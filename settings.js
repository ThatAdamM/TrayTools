function getBase64Image(img) {
  var canvas = document.createElement("canvas");
  canvas.width = img.width;
  canvas.height = img.height;
  var ctx = canvas.getContext("2d");
  ctx.drawImage(img, 0, 0);
  var dataURL = canvas.toDataURL("image/png");
  return dataURL;/*.replace(/^data:image\/?[A-z]*;base64,/)*/;
}
window.onload = async () => {
  document.querySelector("#newiconimg").addEventListener("click", () => newIcon("Image"))
document.querySelector("#newiconemo").addEventListener("click", () => newIcon("Emoji"))
document.querySelector("#newiconana").addEventListener("click", () => newIcon("time analogue"))
document.querySelector("#newiconcol").addEventListener("click", () => newIcon("colour"))
document.querySelector("#newiconbla").addEventListener("click", () => newIcon("blank"))
  let x = document.getElementById("container")
  let j = await window.process.getIcons();
  j.trayIcons.forEach((i, index) => {
    x.innerHTML += `
    <div class="wrap-collabsible collapsible-${index}"> <input id="collapsible${index}" class="toggle" type="checkbox"> <label for="collapsible${index}"
    class="lbl-toggle">Icon ${index + 1}</label>
    <div class="collapsible-content">
    <div class="content-inner">
    <form>
  <div class="row">
    <div class="col">
      Current icon: <img id="imgIcon${index}" src="${i.icon}" height="20px" width="20px">
    </div>
    <div class="col">
      <a href="javascript:getImage(${index})"><button type="button" class="btn btn-light">Choose a New Image</button></a>
    </div>
  </div>
  <p class="divider line one-line"> Left Click: </p><br>
  <div class="row">
    <div class="col">
      <label class="form-label">Left click action type:</label>
      <select class="form-select"  name="Action" id="leftActions${index}">
          <option value="nout" ${i.clickType == 'nout' ? "selected" : ""}>Do Nothing</option>
          <option value="open" ${i.clickType == 'open' ? "selected" : ""}>Open a File, URL or Program</option>
          <option value="popup" ${i.clickType  == 'popup' ? "selected" : ""}>Show a message</option>
          <option value="notification" ${i.clickType == 'notification' ? "selected" : ""}>Send a Notifcation</option>
        </select>
    </div>
    <div class="col">
      <label class="form-label">Left click action:</label>
      <input id="leftClickAction${index}" value="${i.click}" type="text" class="form-control" placeholder="Left-Click Action">
    </div>
  </div>
  <p class="divider line one-line"> Right Click Menu: </p><br>
  <a href="javascript:addAction(${index})"><button type="button" class="btn btn-primary">Add another action... </button></a>
  <div id="actions${index}">
  </div>
  <p class="divider line one-line"> Finalise: </p><br>
  <div class="row">
  <div class="col">
  <input id="tooltip${index}" type="text" class="form-control" placeholder="Tooltip (Hover-Over Text)" value="${i.tooltip ?? ''}">
  </div>
  <div class="col">
  <a href="javascript:saveIcon(${index})"><button type="button" class="btn btn-success">Save Icon</button></a>${index == 0 ? "<p>You cannot remove the first icon. Edit it instead!</p>" : `<a href="javascript:remIcon(${index})"><button type="button" class="btn btn-danger">Delete Icon</button></a>`}
  </div>
  </div
</form>
</div>
</div>
</div>`
    /*
    <div class="wrap-collabsible"> <input id="collapsible${index}" class="toggle" type="checkbox"> <label for="collapsible${index}"
    class="lbl-toggle">Icon ${index + 1}</label>
    <div class="collapsible-content">
      <div class="content-inner">
        <p>Type: ${i.type}</p>
        <a href="javascript:getIcon(${index})"><button>Choose a new image</button></a>
        <p>Current Icon: <img src="${i.icon}" height="20px" width="20px"></p>
        <p>Hover-Over Text:</p> <input type="text" value="${i.tooltip}" id="tooltip${index}"/>
        <p>Left Click Action:</p>
        <select name="Action" id="leftActions${index}">
            <option value="nout">Do Nothing</option>
            <option value="open">Open a File, URL or Program</option>
            <option value="popup">Show a message</option>
            <option value="notification">Send a Notifcation</option>
        </select>
        <p>Value for click action: <input type="text" value="${i.click}" placeholder="https://www.google.com" id="leftActionAction-${index}"/></p>
        <p>Right Click Menu:</p>
        <div id="actions${index}">
        </div>
    </div>
</div>
</div>*/
console.log("MENU", i.menu)
if(i.menu == null) return;
let actions = document.getElementById(`actions${index}`)
i.menu.forEach((thing, thingindex) => {
  console.log("Type", thing.type)
  let possibleactions = [null, "text", "open", "popup", "notification"];
  let i2 = possibleactions.indexOf(thing.type);
  
actions.innerHTML += `<div class="rightClickMenuAction">
<p class="divider line one-line"> Menu action ${thingindex + 1}: </p><br>
<div class="row">
<div class="col">
  <a href="javascript:remAction(${index}, ${thingindex})"><button type="button" class="btn btn-danger">Delete Action ${thingindex + 1}</button></a>
  </div>
  <div class="col">
  <select class="form-select" name="Action" id="MenuAction${thingindex}-${index}">
  <option value="null" ${i2 == 0 ? "selected" : ""}>Don't show action</option>
  <option value="text" ${i2 == 1 ? "selected" : ""}>Just show text</option>
  <option value="open" ${i2 == 2 ? "selected" : ""}>Open a File, URL or Program</option>
  <option value="popup" ${i2 == 3 ? "selected" : ""}>Show a message</option>
  <option value="notification" ${i2 == 4 ? "selected" : ""}>Send a Notifcation</option>
  </select>
  </div>
  </div>
  <div class="row">
  <div class="col">
<label class="form-label">Menu action ${thingindex + 1} label:</label> <input class="form-control" type="text" value="${thing.label}" placeholder="Open Google" id="menuAction${thingindex}Name-${index}"/>
</div>
<div class="col">
<label class="form-label">Menu action ${thingindex + 1} value:</label> <input class="form-control" type="text" value="${thing.link}" placeholder="https://www.google.com" id="menuAction${thingindex}Action-${index}"/>
</div>
</div>
</div>`
})
  })
}

async function newIcon() {
  let index = document.querySelectorAll("form").length;
  document.getElementById("container").innerHTML += `
  <div class="wrap-collabsible collapsible-${index}"> <input id="collapsible${index}" class="toggle" type="checkbox"> <label for="collapsible${index}"
  class="lbl-toggle">Icon ${index + 1}</label>
  <div class="collapsible-content">
  <div class="content-inner">
  <form>
<div class="row">
  <div class="col">
    Current icon: <img id="imgIcon${index}" src="data:image/png;base64,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" height="20px" width="20px">
  </div>
  <div class="col">
    <a href="javascript:getImage(${index})"><button type="button" class="btn btn-light">Choose a New Image</button></a>
  </div>
</div>
<p class="divider line one-line"> Left Click: </p><br>
<div class="row">
  <div class="col">
    <label class="form-label">Left click action type:</label>
    <select class="form-select"  name="Action" id="leftActions${index}">
          <option value="nout">Do Nothing</option>
          <option value="open">Open a File, URL or Program</option>
          <option value="popup">Show a message</option>
          <option value="notification">Send a Notifcation</option>
      </select>
  </div>
  <div class="col">
    <label class="form-label">Left click action:</label>
    <input id="leftClickAction${index}" value="" type="text" class="form-control" placeholder="Left-Click Action">
  </div>
</div>
<p class="divider line one-line"> Right Click Menu: </p><br>
<a href="javascript:addAction(${index})"><button type="button" class="btn btn-primary">Add another action... </button></a>
<div id="actions${index}">
</div>
<p class="divider line one-line"> Finalise: </p><br>
<div class="row">
<div class="col">
<input id="tooltip${index}" type="text" class="form-control" placeholder="Tooltip (Hover-Over Text)" value="">
</div>
<div class="col">
<a href="javascript:saveIcon(${index})"><button type="button" class="btn btn-success">Save Icon</button></a><a href="javascript:remIcon(${index})"><button type="button" class="btn btn-danger">Delete Icon</button></a>
</div>
</div
</form>
</div>
</div>
</div>`
}

async function getImage(num) {
  let i2 = await window.process.openFile();
  if(!i2) return;
  let base64 = await window.process.toBase64(i2);
  console.log(base64)
  document.getElementById("imgIcon" + num).src = base64;
}

function addAction(index3) {
  let thingindex = document.querySelectorAll("#actions" + index3 + " > .rightClickMenuAction").length;
  document.getElementById("actions" + index3).innerHTML += `<div class="rightClickMenuAction">
  <p class="divider line one-line"> Menu action ${thingindex + 1}: </p><br>
  <div class="row">
  <div class="col">
    <a href="javascript:remAction(${index3}, ${thingindex})"><button type="button" class="btn btn-danger">Delete Action ${thingindex + 1}</button></a>
    </div>
    <div class="col">
    <select class="form-select" name="Action" id="MenuAction${thingindex}-${index3}">
    <option value="null" selected>Don't show action</option>
    <option value="text">Just show text</option>
    <option value="open">Open a File, URL or Program</option>
    <option value="popup">Show a message</option>
    <option value="notification">Send a Notifcation</option>
    </select>
    </div>
    </div>
    <div class="row">
    <div class="col">
  <label class="form-label">Menu action ${thingindex + 1} label:</label> <input class="form-control" type="text" placeholder="Open Google" id="menuAction${thingindex}Name-${index3}"/>
  </div>
  <div class="col">
  <label class="form-label">Menu action ${thingindex + 1} value:</label> <input class="form-control" type="text" placeholder="https://www.google.com" id="menuAction${thingindex}Action-${index3}"/>
  </div>
  </div>
  </div>`
}

function remIcon(index15) {
  document.querySelector(".collapsible-" + index15).remove();
  window.process.remIcon(index15)
}

function remAction(index3, thingindex2) {
  document.querySelectorAll(`#actions${index3} > .rightClickMenuAction`)[thingindex2].remove();
}
/*
<form>
  <div class="row">
    <div class="col">
      Current icon: <img id="imgIcon0" src="C:/Users/adaml/Desktop/SSH Client/twemoji/objects/1f9f9.png" height="20px" width="20px">
    </div>
    <div class="col">
      <a href="javascript:getImage(0)"><button type="button" class="btn btn-light">Choose a New Image</button></a>
    </div>
  </div>
  <p class="divider line one-line"> Left Click: </p><br>
  <div class="row">
    <div class="col">
      <label class="form-label">Left click action type:</label>
      <select class="form-select" name="Action" id="leftActions0">
            <option value="nout">Do Nothing</option>
            <option value="open">Open a File, URL or Program</option>
            <option value="popup">Show a message</option>
            <option value="notification">Send a Notifcation</option>
        </select>
    </div>
    <div class="col">
      <label class="form-label">Left click action:</label>
      <input type="text" class="form-control" placeholder="Left-Click Action">
    </div>
  </div>
  <p class="divider line one-line"> Right Click Menu: </p><br>
  <a href="javascript:addAction(0)"><button type="button" class="btn btn-primary">Add another action... </button></a>
  <div id="actions0">
  <div class="rightClickMenuAction">
  <p class="divider line one-line"> Menu action 1: </p><br>
  <div class="row">
  <div class="col">
    <a href="javascript:remAction(0, 0)"><button type="button" class="btn btn-danger">Delete Action 1</button></a>
    </div>
    <div class="col">
    <select class="form-select" name="Action" id="MenuAction0-0">
    <option value="null" selected="">Don't show action</option>
    <option value="text">Just show text</option>
    <option value="open">Open a File, URL or Program</option>
    <option value="popup">Show a message</option>
    <option value="notification">Send a Notifcation</option>
    </select>
    </div>
    </div>
    <div class="row">
    <div class="col">
  <label class="form-label">Menu action 1 label:</label> <input class="form-control" type="text" placeholder="Open Google" id="menuAction1Name-0">
  </div>
  <div class="col">
  <label class="form-label">Menu action 1 value:</label> <input class="form-control" type="text" placeholder="https://www.google.com" id="menuAction1Action-0">
  </div>
  </div>
  </div></div>
  <p class="divider line one-line"> Finalise: </p><br>
  <div class="row">
  <div class="col">
  <input type="text" class="form-control" placeholder="Tooltip (Hover-Over Text)" value="Welcome to TrayTools!">
  </div>
  <div class="col">
  <a href="javascript:saveIcon(0)"><button type="button" class="btn btn-success">Save Icon</button></a>
  </div>
  </div>
</form>
*/
async function saveIcon(index3) {
  //Can get icon from image src and just slice the URL off
  let iconBase64 = document.querySelector("#imgIcon" + index3).src;
  let leftActionType = document.querySelector("#leftActions" + index3).value;
  let leftActionAction = document.querySelector("#leftClickAction" + index3).value;
  /*
    From tray.json. We'll just make it up here since it'll be SO much easier xD
    { "label": "My epik programs", "type": "text"},
    { "label": "Open Calc", "type": "open", "link": "calc.exe" }, 
    { "label": "Open Brave", "type": "open", "link": "brave://newtab " }
  */
  let rightActions = [];
  let menuactions = document.querySelectorAll("#actions" + index3 + " > .rightClickMenuAction")
  let thingy = 0;
  for (const a of menuactions) {
    let type = await document.querySelector("#MenuAction" + thingy + "-" + index3).value;
    let label = await document.querySelector("#menuAction" + thingy + "Name-" + index3).value;
    let link = await document.querySelector("#menuAction" + thingy + "Action-" + index3).value;
    rightActions.push({ label: label, type: type, link: link});
    thingy++;
  }
  let tooltip = document.querySelector("#tooltip" + index3).value;


  /*
    Again, from tray.json. Here's the structure of what we save.
    {
            "type": "Emoji",
            "icon": "C:/Users/adaml/Desktop/SSH Client/twemoji/objects/1f9f9.png",
            "click": "calc.exe",
            "menu": [Menu from before...],
            "tooltip": "Welcome to TrayTools!"
    },
  */
 console.log(iconBase64)
  window.process.editIcon(index3, {
    type: "Image",
    icon: iconBase64,
    click: leftActionAction,
    clickType: leftActionType,
    menu: rightActions,
    tooltip: tooltip
  })

}