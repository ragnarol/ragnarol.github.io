#!/usr/bin/env node
// Generate search.json files for each language
// Run: node scripts/generate-search-index.js

const fs = require('fs');
const path = require('path');

const SOURCE_DIR = path.join(__dirname, '..');

function cleanTitle(filepath) {
  const title = path.basename(filepath, '.md');
  let result = title
    .replace(/ - eng$/, '')
    .replace(/ - summary$/, '')
    .replace(/- summary$/, '')
    .replace(/\s+-\s+/g, ' - ')
    .replace(/_/g, ' ')
    .replace(/\s{2,}/g, ' ')
    .trim();
  return result;
}

function stripContent(content) {
  return content
    .replace(/#{4,}/g, '')       // code fences
    .replace(/`/g, '')           // inline code
    .replace(/\*\*(.*?)\*\*/g, '$1')   // bold
    .replace(/\*(.*?)\*/g, '$1')     // italic
    .replace(/---+/g, '')         // horizontal rules
    .replace(/#{1,6}\s+/g, '')    // headers
    .replace(/^\s*[-*+]\s+/gm, '') // list items
    .replace(/\n+/g, ' ')         // newlines to spaces
    .trim();
}

function getCampaign(relPath) {
  const parts = relPath.split('/');
  return parts.length > 1 ? parts[1] : 'general';
}

function findMdFiles(dir, baseDir) {
  const files = [];
  const entries = fs.readdirSync(dir, { withFileTypes: true });
  for (const entry of entries) {
    const fullPath = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      files.push(...findMdFiles(fullPath, baseDir));
    } else if (entry.isFile() && entry.name.endsWith('.md')) {
      files.push(fullPath);
    }
  }
  return files;
}

const languages = ['en', 'es'];

languages.forEach((lang) => {
  const langDir = path.join(SOURCE_DIR, lang);
  if (!fs.existsSync(langDir)) {
    console.log(`Language directory not found: ${lang}`);
    return;
  }

  const files = findMdFiles(langDir, langDir);
  const pages = [];

  files.forEach((filepath) => {
    const basename = path.basename(filepath);
    if (basename === 'index.md') return;

    const relPath = path.relative(SOURCE_DIR, filepath);
    const url = '/' + relPath.replace(/\.md$/, '').replace(/\\/g, '/');

    const contentRaw = fs.readFileSync(filepath, 'utf8');
    const content = stripContent(contentRaw);
    const title = cleanTitle(filepath);
    const campaign = getCampaign(relPath);

    pages.push({
      title,
      url,
      content,
      campaign,
      snippet: content.substring(0, 300),
    });
  });

  pages.sort((a, b) => a.title.localeCompare(b.title));

  const outputDir = langDir;
  fs.mkdirSync(outputDir, { recursive: true });
  const outputFile = path.join(outputDir, 'search.json');
  fs.writeFileSync(outputFile, JSON.stringify(pages, null, 2));
  console.log(`Generated: ${outputFile} (${pages.length} pages)`);
});

console.log('Done!');
