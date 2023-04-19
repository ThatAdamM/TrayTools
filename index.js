const { app, BrowserWindow, ipcMain, Tray, dialog, shell, nativeImage, Menu, Notification, screen } = require('electron');
const { exec } = require("child_process");
let { clock } = require("./clock.js")
let path = require("path")
let currentlyOpenTrays = [];
let win;
let fs = require("fs").promises;
app.on('window-all-closed', () => {
  // Do nothing. YAY :D
})

const PATH_NAME = path.join(__dirname, "./tray.json")

function setTrays() {
  if(currentlyOpenTrays.length > 0) currentlyOpenTrays.forEach((e) => {
    e.destroy()
    e.emit("destroy")
  })
  const json = require(PATH_NAME);
  json.trayIcons.forEach(async (element) => {
    let x = null;
    if(element.icon == "time analogue") {
      let a = await clock()
      x = new Tray(nativeImage.createFromDataURL(a));
      let oldd = new Date()
      x.setToolTip(oldd.getHours() + ":" + oldd.getMinutes())
      let inter = setInterval(async () => {
        x.setImage(nativeImage.createFromDataURL(await clock()))
        let d = new Date()
        x.setToolTip(d.getHours() + ":" + d.getMinutes())
      }, 60000)
      x.on('destroy', () => {
        clearInterval(inter);
      })
    } else {
      x = new Tray(nativeImage.createFromDataURL(element.icon));
    }
    if(element.click) {
      x.on("click", () => {
        if(element.clickType == "nout") {
          return;
        } else if(element.clickType == "open") {
          shell.openPath(element.click)
        } else if(element.clickType == "popup") {
          dialog.showMessageBox({ type: "info", icon: "./logo.png", title: "TrayTools", message: element.click})
        } else if(element.clickType == "notification") {
          new Notification({body: element.click}).show()
        }
      })
    }
    let temp = [];
    if(element.menu) {
      element.menu.forEach((i) => {
        if(i.type == "open") {
          temp.push({label: i.label, click: () => {shell.openPath(i.link)}})
        } else if(i.type == "text") {
          temp.push({label: i.label})
        } else if(i.type == "separator") {
          temp.push({type: "separator"})
        } else if(i.type == "notification") {
          new Notification({body: element.click}).show()
        } else if(i.type == "popup") {
          dialog.showMessageBox({ type: "info", icon: "./logo.png", title: "TrayTools", message: element.click})
        }
      })
    }
    if(temp != []) temp.push({type: "separator"})
    temp.push({label: "Settings", click: () => {
      if(win) {
        win.focus();
      } else {
      
      Menu.setApplicationMenu(null)

      win = new BrowserWindow({
        width: 800,
        height: 600,
        webPreferences: {
          preload: path.join(__dirname, 'settings-preload.js')
        },
        resizable: false,
        maximizable: false,
        icon: "logo.png"
      })
      win.loadFile("settings.html")
      win.on("close", () => { win = null;})
      win.webContents.openDevTools()
    }
    }})
    temp.push({label: "Welcome Screen", click: () => new BrowserWindow({
      width: 600,
      height: 312,
      resizable: false,
      maximizable: false,
      icon: "logo.png",
      frame: false,
      webPreferences: {
        preload: path.join(__dirname, 'welcome-preload.js')
      },
    }).loadFile("./welcome/home.html")})
    temp.push({label: "Quit TrayTools", click: () => {app.quit()}})
    temp.push({label: "Refresh Icons", click: () => {setTrays()}})
    x.setContextMenu(Menu.buildFromTemplate(temp))
    if(element.tooltip && element.type != "time analogue") {
      x.setToolTip(element.tooltip)
    }
    currentlyOpenTrays.push(x)
  });
}

const start = () => {
  let j = require(PATH_NAME)
  if(j.showStartupMessage == true) {
    win = new BrowserWindow({
      width: 800,
      height: 600,
      webPreferences: {
        preload: path.join(__dirname, 'preload.js')
      }
    })
    win.on("close", () => { win = null;})
    win.loadFile('welcome.html')
  }
  setTrays()
}

/**
* Saves data to a JSON file.
* This is a promise.
* @param data - The data to set in a file
* @param file - The file name to set.
* @returns - JSON Object representing the data passed to it
*/
async function saveJson(data, file) {
  try {
    const fs = require("fs").promises;
    await fs.writeFile(PATH_NAME, JSON.stringify(data));
    console.log("Writing to: ", PATH_NAME)
    return data;
  } catch (e) {
    throw e;
  }
}

/**
 * Should only be used internally :)
 */
async function handleFileOpen(event, path2) {
  let fullpath = path.join(__dirname, path2 || "/")
  const { canceled, filePaths } = await dialog.showOpenDialog({ defaultPath: fullpath, title: "Select an image as an icon", buttonLabel: "Use Image", filters: [{ name: "Images and Icons", extensions: ["ico", "png", "jpg"]}], properties: ["openFile", "dontAddToRecent"]})
  if (canceled) {
    return
  } else {
    return filePaths[0]
  }
}

async function toBase64(event, img) {
  let file = await fs.readFile(img)
  const imageType = require('image-type')
  const type = imageType(file)
  let b64 = file.toString('base64')
  return `data:${type.mime};base64,${b64}`;
}

/**
 * Should only be used internally :)
 */
async function handleGetIcons(event) {
  const json2 = require(PATH_NAME);
  return json2;
}

/**
 * Should only be used internally :)
 */
async function handleSetIcon(event, dat) {
  let j = require(PATH_NAME);
  j.trayIcons.push(dat);
  await saveJson(j, PATH_NAME);
}

/**
 * Should only be used internally :)
 */
async function handleEditIcon(event, num, dat) {
  let j = require(PATH_NAME);
  j.trayIcons[num] = dat;
  await saveJson(j, PATH_NAME);
}

/**
 * Should only be used internally :)
 */
async function handleRemIcon(event, num) {
  let j = require(PATH_NAME);
  j.trayIcons.splice(num, 1);
  await saveJson(j, PATH_NAME);
}

ipcMain.handle('dialog:openFile', handleFileOpen)
ipcMain.handle('getIcons', handleGetIcons)
ipcMain.on("setIcon", handleSetIcon)
ipcMain.on("editIcon", handleEditIcon)
ipcMain.on("remIcon", handleRemIcon)
ipcMain.on("openJSON", () => shell.openPath(PATH_NAME))
ipcMain.on("refresh", () => setTrays())
ipcMain.handle('toBase64', toBase64)

ipcMain.on("welcomeStartup", (event, d) => {
  app.setLoginItemSettings({
    openAtLogin: d
  })
  console.log("Run on startup is now set to ", d)
});

ipcMain.on("welcomeOnLoad", (event, d) => {
  let jsonthingagain = require(PATH_NAME);
  jsonthingagain.welcomeOnLoad = d;
  saveJson(jsonthingagain, PATH_NAME)

  console.log("Welcome On Load is now set to ", d)
})

app.whenReady().then(() => {
  const jsonythingy = require(PATH_NAME);
if((jsonythingy.usedBefore == false) || (jsonythingy.welcomeOnLoad == true)) {
new BrowserWindow({
  width: 600,
  height: 312,
  resizable: false,
  maximizable: false,
  icon: "logo.png",
  frame: false,
  webPreferences: {
    preload: path.join(__dirname, 'welcome-preload.js')
  },
}).loadFile("./welcome/home.html");

jsonythingy.usedBefore = true;
saveJson(jsonythingy, PATH_NAME)
}

  start()
})



