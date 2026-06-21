const fs = require('fs');
const path = require('path');
const changelogPath = path.join(__dirname, '../../CHANGELOG.md');
const packageJsonPath = path.join(__dirname, '../package.json');
try {
  const changelog = fs.readFileSync(changelogPath, 'utf8');
  const versionMatch = changelog.match(/## \[([0-9]+\.[0-9]+\.[0-9]+)\]/);
  if (versionMatch) {
    const version = versionMatch[1];
    const pkg = JSON.parse(fs.readFileSync(packageJsonPath, 'utf8'));
    if (pkg.version !== version) {
      pkg.version = version;
      fs.writeFileSync(packageJsonPath, JSON.stringify(pkg, null, 2) + '\n');
      console.log('Synchronized frontend version to ' + version + ' based on CHANGELOG.md');
    }
  }
} catch (e) {
  console.warn('Could not sync version from changelog:', e.message);
}
