const fs = require("fs");
const yaml = require("js-yaml");

const filePath = "../../Resources/Changelog/GabyChangelog.yml"

function main() {
    const file = fs.readFileSync(filePath, "utf-8");

    const data = yaml.load(file);
    const entries = data && data.Entries ? Array.from(data.Entries) : [];

    // entries.sort((a, b) => (new Date(b.time)) - (new Date(a.time)));

    for (const i in entries) {
        entries[i].id = Number(i);
    }

    fs.writeFileSync(
        filePath,
        "Entries:\n" + yaml.dump(data.Entries, { indent: 2 }).replace(/^---/, "")
    )
}

main()
