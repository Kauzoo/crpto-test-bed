import { ZkSendLinkBuilder, ZkSendLink } from '@mysten/zksend';
import * as fs from 'fs';
import * as NodeSchedule from 'node-schedule';

let codes = [];
//codes.push("https://getstashed.com/claim#$YGD4/tyt+Cx84s1WyZvm3EsW2QVsS5NdSvP83FfVgpE=");
codes.push("https://getstashed.com/claim#$pacC4eRW1vwqGkscytI3XJ5aQR0LTovBcEqByVTT87Q=");
codes.push("https://getstashed.com/claim#$UinedgfJLs4BSuXyeaYnP4uOjsCEOA6mtf2ihJU2PRU=");
//codes.push("https://getstashed.com/claim#$uWYi7BJmuetQK3u++zRHPAfME9XOmGUKEqThrVZrbLw=");
codes.push("https://getstashed.com/claim#$g6wbuL0VQnTNnqrHvttbfY3rChlX9+s2dsawzw2gwxU=");
codes.push("https://getstashed.com/claim#$NDyBRP3gCGTV32KRcvojKiUthA56LeluQZK/g5cuDJQ=");
codes.push("https://getstashed.com/claim#$lyDKa1OhauvupnKcrUpkQ1Pp6za2ksKyFotdTLc7U1U=");
codes.push("https://getstashed.com/claim#$Kxjdyj4ceKcfE+5RFuPBArLy5OaR7qWjbXF294k+YS4=");
codes.push("https://getstashed.com/claim#$1/q+1eWaaBYlkuh+8AzylXpSnr8OxHY6HKqXlaq2SVg=");
codes.push("https://getstashed.com/claim#$3ohwxaJGGTYTGidzARae1wuclLwh+3GJULw80KO/yMc=");

const CLAIMED_FILE = 'R:/claims.json';
let claimedLinks = new Set(); // To keep track of claimed links
const MAX_RETRIES = 5;

// Load previously claimed links if file exists
if (fs.existsSync(CLAIMED_FILE)) {
    try {
        const data = fs.readFileSync(CLAIMED_FILE, 'utf-8');
        const parsed = JSON.parse(data);
        parsed.forEach(item => {
            if (item.claimed) {
                claimedLinks.add(item.link);
            }
        });
        console.log(`Loaded ${claimedLinks.size} previously claimed links.`);
    } catch (error) {
        console.error(`Error loading claims file: ${error.message}`);
    }
}

// Function to handle rate-limited requests
async function fetchLinkStatus(url, retries = 0) {
    try {
        let tlink = await ZkSendLink.fromUrl(url);
        console.log(`Successfully fetched status for ${url}`);
        return tlink;
    } catch (error) {
        if (error.response && error.response.status === 429) {
            if (retries < MAX_RETRIES) {
                const delay = Math.pow(2, retries) * 1000; // Exponential backoff
                console.log(`Rate limited. Retrying ${url} in ${delay} ms...`);
                await new Promise(resolve => setTimeout(resolve, delay));
                return fetchLinkStatus(url, retries + 1);
            } else {
                console.error(`Failed to fetch ${url} after ${MAX_RETRIES} retries: ${error.message}`);
                return null;
            }
        } else {
            console.error(`Error fetching ${url}: ${error.message}`);
            return null;
        }
    }
}

// Schedule job to run every 5 seconds
NodeSchedule.scheduleJob('*/5 * * * * *', async () => {
    console.log('Starting new check cycle...');
    let rink = [];
    let checkedCount = 0;

    for (let i = 0; i < codes.length; i++) {
        if (claimedLinks.has(codes[i])) {
            console.log(`Skipping already claimed link: ${codes[i]}`);
            continue; // Skip already claimed links
        }

        let tlink = await fetchLinkStatus(codes[i]);
        if (tlink) {
            let claimed = tlink.claimed ? true : false;
            rink.push({link:codes[i], claimed:claimed});
            if (claimed) {
                claimedLinks.add(codes[i]);
                console.log(`Link claimed: ${codes[i]}`);
            }
        }

        checkedCount++;
    }

    try {
        fs.writeFileSync(CLAIMED_FILE, JSON.stringify(rink, null));
        console.log(`Updated claims file with ${rink.length} entries.`);
    } catch (error) {
        console.error(`Error writing to claims file: ${error.message}`);
    }

    console.log(`Checked ${checkedCount} links in this cycle. Total claimed links: ${claimedLinks.size}`);
});