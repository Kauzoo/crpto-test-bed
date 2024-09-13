import { ZkSendLinkBuilder, ZkSendLink } from '@mysten/zksend';
import * as fs from 'fs';
import * as NodeSchedule from 'node-schedule';

let codes = [];

codes.push("https://getstashed.com/claim#$W+aMQOAzMEs/rpR1bWiwkRzCuJ7KHtFuvXNEsEge9Vw=");
codes.push("https://getstashed.com/claim#$57S+uI2jiGksy1jGFgEiQj5QAiR7hi5Ka37BzegXBL0=");
codes.push("https://getstashed.com/claim#$bYzUGdhQIWdmcHgjbJQ9VLHocOdKRcPb1QE8POIW6RI=");
codes.push("https://getstashed.com/claim#$m0joQ7TNE7Jo+XTUS70T99utHANwTi0wXBGapMITWd8=");
codes.push("https://getstashed.com/claim#$cJ8Ium0D7OHfLyuIiVuvgUGk6RpenXAx4dEkr5sfzs8=");
codes.push("https://getstashed.com/claim#$3I9QPGJTb5xcBZ9jhx0hnp9ByDONQmYexY2aojIEMG8=");
codes.push("https://getstashed.com/claim#$d1mstS5FYMGN0p01w0UvUl+0k3zMdFlANPsST/rB6xU=");
codes.push("https://getstashed.com/claim#$pf294buZ/MJkpahVGWennc52OlsRMgk5VtOiTsrs7wk=");
codes.push("https://getstashed.com/claim#$yOGAggOkiygtdlbkVEcuuODKwofY822T6f9ogWV35JU=");
codes.push("https://getstashed.com/claim#$08olMBv7vl7vMFsuDnJLzmJPFI0qgORQfaN4sKPIlRM=");
codes.push("https://getstashed.com/claim#$wite+/rIHmJS0/69eEjBGcgeX7+pxaM77hQqX2r+lBE=");
codes.push("https://getstashed.com/claim#$YlyWi/U/YMNOeEdN3Rb5puMdzHi+sGWmx849wsO1JNs=");
codes.push("https://getstashed.com/claim#$HKvtsNVkmmLz5yXcxFxxMSCyZflTS5j5P7AL1JbyM/o=");
codes.push("https://getstashed.com/claim#$y056yG6ONoDNv0xTCcQvVQMBrRlL4Mj04egYhwpvaR8=");
codes.push("https://getstashed.com/claim#$p4qzrSvIg4vNiScRhaz/aUCBsllgsCmw2QYS+vI4NvY=");
codes.push("https://getstashed.com/claim#$Do5v1kOzgqSfVoBEHoFChsYiez/Zg6n03MLrELoeUKA=");
codes.push("https://getstashed.com/claim#$ZS0NfXgCy22XADYJk0IgNJluUuw6okxOzq0Lk+c6vCg=");
codes.push("https://getstashed.com/claim#$bT1H37j6HKRlJ2hfibJ0qNH1AIBQilM007OkzB2nnjE=");
codes.push("https://getstashed.com/claim#$r+mIyQFI4bXn4/Jei0Cun0GUOx+/1kX2Hkkxxm7vccs=");
codes.push("https://getstashed.com/claim#$Vjs9+aemVuL1CwHlNexWfx48U43eUD5CzgIjv4EIlW4=");
codes.push("https://getstashed.com/claim#$1zKjAXfLTJcIhgUO1dWaWDw9KW8ljJgw0mUoH6MNaxA=");
codes.push("https://getstashed.com/claim#$K8zI9kS8UkaTh6cRHl1Hc2tJcgF7yTLmViy6u9XC8q4=");
codes.push("https://getstashed.com/claim#$GUY0JxsuPzC6u5+5+NdjesMnVDVf8D5SQS/41eKqvzM=");
codes.push("https://getstashed.com/claim#$9p9XuRSQYiNE2P1WYhSlXoEzZ37Izh6csVMma/DvENA=");
codes.push("https://getstashed.com/claim#$Qsac91KOKr7ZIRo4cDLMHWY9/OboFF2djYTHyDPKAEk=");
codes.push("https://getstashed.com/claim#$QgBgr6qMZU7XH0mbDCDN7fYLXrpDo93GAYsWZGaR+Gg=");
codes.push("https://getstashed.com/claim#$OkIekcZbgLyvMOzNRkCJ3Sh/XtflaRPsQSnYEmcm+1M=");
codes.push("https://getstashed.com/claim#$7fYpTwaL3xwxxJQEZpAGSTC4ZZempHIad5OUOPRgvWw=");
codes.push("https://getstashed.com/claim#$+xyKUldbcpfun+YNG8i7rqkqkO5j2/Xzvm1HuVrey5I=");
codes.push("https://getstashed.com/claim#$KDDsHSbWUDILrR6d3gFXIssqeVTn67Sq2dl0cipyGu8=");
codes.push("https://getstashed.com/claim#$AAyJz/WgMjiCfjHkIlcfeFktHRi9eGJrqJT2cce2e5A=");
codes.push("https://getstashed.com/claim#$sWrenoIfuhS/9chhpzRdJ16qmZGERIF3r1rjOcS3lrY=");
codes.push("https://getstashed.com/claim#$D9MMbwoUVvrINLQ33mI6Yp7h5ZMUDbD2iSMrHgvOhU0=");
codes.push("https://getstashed.com/claim#$BU5PaXyq59vFH4YFA3oj7rUQRXXaSrCJFPAmnJzttX4=");
codes.push("https://getstashed.com/claim#$wf8d5cVREMoA36hr9mk3aQnDMcWv9q4LPJ4N9sPkZVY=");
codes.push("https://getstashed.com/claim#$GrHWbc5i2puIK+x3fqVwwb18OaU7AolU55UNZPCniuI=");
codes.push("https://getstashed.com/claim#$Iruzkc2PkfP8YvORsL4ELoqdmh39ZGjkBYd9TJUYcW0=");
codes.push("https://getstashed.com/claim#$ENZXFq3KgkGaRQxoqpOvrEWfca4FRDFkIkt3UBbAC+4=");
codes.push("https://getstashed.com/claim#$rv9Wvjdl0ZaDbTJY/80jxSBXkNTwtrhBSkbC8A5dvKw=");
codes.push("https://getstashed.com/claim#$5gf6dpxjGrf+fqous3NoVa1dGco39GG7sRlC5teNsRQ=");
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