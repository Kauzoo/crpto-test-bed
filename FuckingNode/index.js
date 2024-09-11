import { ZkSendLinkBuilder, ZkSendLink } from '@mysten/zksend';
import * as fs from 'fs';
import * as NodeSchedule from 'node-schedule';

let codes = [];
codes.push("https://getstashed.com/claim#$8A8Krv00c9+2qJhZ1+04GS6J+9VXZhfTMWkHRA6129A=");
codes.push("https://getstashed.com/claim#$3OSbk9k7r7ytPjB9RGDnJD4Pmg/WPxD2QWsqoCfCr1s=");
codes.push("https://getstashed.com/claim#$MVpMofoXBmLeZYpUklHZAw03TrC/ENRd0DFPT5TJw+0=");
codes.push("https://getstashed.com/claim#$nH+1elBUBKXAEqjLqrNw3Y79ayutenT5lrRg/PnwFo8=");

// TODO: real links
let counter = 0;

NodeSchedule.scheduleJob('*/1 * * * * *', async () => {
    let rink = [];
    for (let i = 0; i < codes.length; i++) {
        let tlink = await ZkSendLink.fromUrl(codes[i]);
        rink.push({link:codes[i], claimed:tlink.claimed})
    }
    console.log(rink);
    fs.writeFileSync('R:/claims.json', JSON.stringify(rink));
    counter++;
    console.log(counter);
    
})
/*
let rink = [];
for (let i = 0; i < codes.length; i++) {
    let tlink = await ZkSendLink.fromUrl(codes[i]);
    rink.push({link:codes[i], claimed:tlink.claimed})
}
console.log(rink);
fs.writeFileSync('R:/claims.json', JSON.stringify(rink));
*/