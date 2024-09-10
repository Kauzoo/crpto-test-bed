import { ZkSendLinkBuilder, ZkSendLink } from '@mysten/zksend'

const linkUrl = "https://getstashed.com/claim#$8A8Krv00c9+2qJhZ1+04GS6J+9VXZhfTMWkHRA6129A=";
const link = await ZkSendLink.fromUrl(linkUrl);
console.log(await link.claimed);