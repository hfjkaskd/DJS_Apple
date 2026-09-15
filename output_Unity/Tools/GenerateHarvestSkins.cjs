/** Offline, editable SVG authoring for the approved harvest UI. No runtime painting. */
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const sharp = require(process.env.HARVEST_SHARP_MODULE || 'sharp');
const root = path.resolve(__dirname, '..');
const output = path.join(root, 'Assets/res/local/harvest/sprite');
const source = path.join(__dirname, 'HarvestSkinSources');
fs.mkdirSync(output, { recursive: true });
fs.mkdirSync(source, { recursive: true });
function svg(w, h, body) {
  return `<svg xmlns="http://www.w3.org/2000/svg" width="${w}" height="${h}" viewBox="0 0 ${w} ${h}">${body}</svg>`;
}
function rounded(w, h, r, colors, stroke, edge, glossy = false, depth = 4) {
  const x = 5, y = 4, rw = w - 10, rh = h - 10 - depth;
  return svg(w, h, `<defs><linearGradient id="fill" x2="0" y2="1"><stop stop-color="${colors[0]}"/><stop offset=".46" stop-color="${colors[1]}"/><stop offset="1" stop-color="${colors[2]}"/></linearGradient><linearGradient id="shine" x2="0" y2="1"><stop stop-color="white" stop-opacity=".78"/><stop offset=".6" stop-color="white" stop-opacity=".1"/><stop offset="1" stop-color="white" stop-opacity="0"/></linearGradient></defs>
  <rect x="${x}" y="${y+depth}" width="${rw}" height="${rh}" rx="${r}" fill="${stroke}" opacity=".22"/>
  <rect x="${x}" y="${y}" width="${rw}" height="${rh}" rx="${r}" fill="url(#fill)" stroke="${stroke}" stroke-width="${edge}"/>
  <rect x="${x+edge}" y="${y+edge}" width="${rw-edge*2}" height="${rh-edge*2}" rx="${r-edge}" fill="none" stroke="url(#shine)" stroke-width="${glossy?4:2}"/>
  ${glossy ? `<path d="M ${x+r*.44} ${y+r*.31} q 4 -8 17 -7 q 5 2 0 6 q -8 2 -13 6 q -5 1 -4 -5" fill="white" opacity=".94"/><ellipse cx="${w-r*.44}" cy="${h-r*.42-depth}" rx="3.5" ry="2" fill="white" opacity=".55"/>` : ''}`);
}
const jobs = [];
function add(name, w, h, border, content) { jobs.push({name,w,h,border,content}); }
add('PanelSkin',512,512,[106,106,106,106],rounded(512,512,97,['#fffef4','#fffdef','#fffbed'],'#ce7a31',6,false,6));
add('HudPanelSkin',384,256,[64,64,64,64],rounded(384,256,56,['#fffef4','#fffdef','#fffbed'],'#ce7a31',5,false,5));
add('InsetSkin',256,256,[49,49,49,49],rounded(256,256,43,['#fff1d7','#fff0d5','#ffedce'],'#efd0a6',3,false,0));
add('TierSkin',256,192,[50,50,50,50],rounded(256,192,42,['#fffef6','#fffdf0','#fff9e6'],'#edb571',4,false,5));
add('TierSelectedSkin',256,192,[50,50,50,50],rounded(256,192,42,['#e6ffc7','#dcffad','#cefb98'],'#3ab91b',7,false,5));
add('ButtonGreenSkin',384,192,[73,73,73,73],rounded(384,192,65,['#69ec24','#49d719','#30bd19'],'#299f13',5,true,5));
add('TitleOrangeSkin',384,160,[68,68,68,68],rounded(384,160,60,['#ffb158','#ff983b','#f48123'],'#c56c25',5,true,5));
add('SecondarySkin',256,112,[44,44,44,44],rounded(256,112,37,['#f5eedf','#f0e6d4','#eaddc6'],'#d9c8ad',3,false,2));
add('ProgressTrackSkin',128,64,[31,25,31,25],rounded(128,64,26,['#e5ccaa','#edd8ba','#f8e9d1'],'#d3b58e',2,false,0));
add('ProgressFillSkin',128,64,[31,25,31,25],rounded(128,64,26,['#a0f751','#67da24','#44bf19'],'#39a313',2,true,0));
function circle(colors, rim, inner = '') {
 return svg(128,128,`<defs><linearGradient id="g" x2="0" y2="1"><stop stop-color="${colors[0]}"/><stop offset=".5" stop-color="${colors[1]}"/><stop offset="1" stop-color="${colors[2]}"/></linearGradient></defs><circle cx="64" cy="67" r="58" fill="${rim}" opacity=".2"/><circle cx="64" cy="62" r="57" fill="url(#g)" stroke="${rim}" stroke-width="4"/><path d="M22 52 A43 43 0 0 1 83 22" stroke="white" stroke-width="4" fill="none" opacity=".65"/>${inner}`);
}
add('StageGraySkin',128,128,[0,0,0,0],circle(['#e8e4e0','#d2ccc7','#c9c3bd'],'#afa39a'));
add('StageGreenSkin',128,128,[0,0,0,0],circle(['#80f342','#47d328','#28ad1a'],'#319f1b'));
add('StageOrangeSkin',128,128,[0,0,0,0],circle(['#ffd06f','#ffb044','#f79024'],'#e07d26'));
add('CloseSkin',128,128,[0,0,0,0],circle(['#ffbb62','#ff9a37','#f38221'],'#c76c25','<path d="M44 42 L84 82 M84 42 L44 82" stroke="#bb6325" stroke-width="18" fill="none" stroke-linecap="round"/><path d="M44 42 L84 82 M84 42 L44 82" stroke="#fffdf0" stroke-width="12" fill="none" stroke-linecap="round"/>'));
add('CheckBadge',128,128,[0,0,0,0],circle(['#83f347','#4ad927','#2ab321'],'#2f9d1c','<path d="M34 64 L54 83 L92 41" stroke="#fffef3" stroke-width="13" fill="none" stroke-linecap="round" stroke-linejoin="round"/>'));
add('ClockSkin',128,128,[0,0,0,0],svg(128,128,'<defs><linearGradient id="c" x2="0" y2="1"><stop stop-color="#e1beff"/><stop offset="1" stop-color="#a779d8"/></linearGradient></defs><circle cx="64" cy="66" r="58" fill="#9470b5" opacity=".2"/><circle cx="64" cy="62" r="57" fill="url(#c)" stroke="#9764c3" stroke-width="3"/><circle cx="64" cy="62" r="43" fill="#fffef6" stroke="#c5a0e4" stroke-width="3"/><path d="M64 30 v5 M96 62 h-5 M64 94 v-5 M32 62 h5" stroke="#c5a0df" stroke-width="3" stroke-linecap="round"/><path d="M64 40 v24 l-17 13" fill="none" stroke="#9160b8" stroke-width="6" stroke-linecap="round" stroke-linejoin="round"/><circle cx="64" cy="63" r="4" fill="#9160b8"/>'));
let rays = '';
for (let i=0;i<16;i++) rays += `<path d="M256 256 L${256+280*Math.cos(i*Math.PI/8-.085)} ${256+280*Math.sin(i*Math.PI/8-.085)} L${256+280*Math.cos(i*Math.PI/8+.085)} ${256+280*Math.sin(i*Math.PI/8+.085)} Z"/>`;
add('RewardRays',512,512,[0,0,0,0],svg(512,512,`<defs><radialGradient id="fade"><stop stop-color="white"/><stop offset=".3" stop-color="white" stop-opacity=".85"/><stop offset="1" stop-color="white" stop-opacity="0"/></radialGradient><mask id="mask"><circle cx="256" cy="256" r="256" fill="url(#fade)"/></mask></defs><circle cx="256" cy="256" r="256" fill="#ffdf62" opacity=".25" mask="url(#mask)"/><g fill="#ffe469" opacity=".66" mask="url(#mask)">${rays}</g>`));
function meta(job) {
 const guid = crypto.createHash('md5').update('djs-apple/harvest-skin-'+job.name).digest('hex');
 const b = job.border;
 return `fileFormatVersion: 2\nguid: ${guid}\nTextureImporter:\n  internalIDToNameTable: []\n  externalObjects: {}\n  serializedVersion: 12\n  mipmaps:\n    mipMapMode: 0\n    enableMipMap: 0\n    sRGBTexture: 1\n  isReadable: 0\n  streamingMipmaps: 0\n  textureFormat: 1\n  maxTextureSize: 512\n  textureSettings:\n    serializedVersion: 2\n    filterMode: 1\n    aniso: 1\n    mipBias: 0\n    wrapU: 1\n    wrapV: 1\n    wrapW: 1\n  nPOTScale: 0\n  compressionQuality: 50\n  spriteMode: 1\n  spriteExtrude: 1\n  spriteMeshType: 1\n  alignment: 0\n  spritePivot: {x: 0.5, y: 0.5}\n  spritePixelsToUnits: 100\n  spriteBorder: {x: ${b[0]}, y: ${b[1]}, z: ${b[2]}, w: ${b[3]}}\n  alphaUsage: 1\n  alphaIsTransparency: 1\n  spriteTessellationDetail: -1\n  textureType: 8\n  textureShape: 1\n  singleChannelComponent: 0\n  flipbookRows: 1\n  flipbookColumns: 1\n  maxTextureSizeSet: 0\n  compressionQualitySet: 0\n  textureFormatSet: 0\n  ignorePngGamma: 0\n  applyGammaDecoding: 0\n  platformSettings:\n  - serializedVersion: 3\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 512\n    resizeAlgorithm: 0\n    textureFormat: -1\n    textureCompression: 0\n    compressionQuality: 50\n    crunchedCompression: 0\n    allowsAlphaSplitting: 0\n    overridden: 0\n  spriteSheet:\n    serializedVersion: 2\n    sprites: []\n    outline: []\n    physicsShape: []\n    bones: []\n    spriteID: ${guid}\n    internalID: 0\n    vertices: []\n    indices:\n    edges: []\n    weights: []\n    secondaryTextures: []\n    nameFileIdTable: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n`;
}
(async()=>{
 for (const job of jobs) {
  fs.writeFileSync(path.join(source,job.name+'.svg'),job.content+'\n');
  await sharp(Buffer.from(job.content),{density:144}).resize(job.w,job.h).png().toFile(path.join(output,job.name+'.png'));
  fs.writeFileSync(path.join(output,job.name+'.png.meta'),meta(job));
 }
 console.log(`Authored ${jobs.length} harvest skins with editable SVG sources and explicit nine-slice borders.`);
})().catch(e=>{ console.error(e);process.exitCode=1; });
