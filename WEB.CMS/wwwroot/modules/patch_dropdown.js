const fs = require('fs');
const path = require('path');

const directory = "i:\\web_car_flow--chi-nhanh-binh-dinh\\WEB.CMS\\wwwroot\\modules";
const files_to_check = [
    "cartcalllist.js",
    "weighedinput.js",
    "registeredvehicle.js",
    "processingIsloading.js",
    "listVehicles.js",
    "cartofactory.js",
    "callthescale.js"
];

for (const filename of files_to_check) {
    const filepath = path.join(directory, filename);
    if (!fs.existsSync(filepath)) {
        console.log(`File not found: ${filename}`);
        continue;
    }

    let content = fs.readFileSync(filepath, 'utf-8');
    
    // Replace position: 'absolute' with 'fixed'
    content = content.replace("position: 'absolute',", "position: 'fixed',");
    
    // Replace the offset calculation at the end
    content = content.replace(
        /left: viewportLeft \+ \$\(window\)\.scrollLeft\(\),\s*top: viewportTop \+ \$\(window\)\.scrollTop\(\),/g,
        "left: viewportLeft,\n            top: viewportTop,"
    );

    fs.writeFileSync(filepath, content, 'utf-8');
    console.log(`Patched ${filename}`);
}
