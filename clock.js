const PImage = require('pureimage')
const Stream = require("node:stream")
const getStream = require("get-stream")

exports.clock = async () => {

const canvas = PImage.make(200, 200)
//create a 2d drawing object
var ctx = canvas.getContext('2d');
//calculate the clock radius by using the height
var radius = canvas.height / 2;
//remap the x and y axis to the center of the canvas
ctx.translate(radius, radius);
//reduce clock radius by 90%
radius = radius * 0.90;

await drawClock()

async function drawClock(){
    drawFace(ctx, radius);
    drawTime(ctx, radius);
}

async function drawFace(ctx, radius){
    var grad;
    //draw white circle for the face
    ctx.beginPath();
    ctx.arc(0,0,radius,0,2*Math.PI);
    // create a radial gradient (inner, middle, and outer edge of clock)
    grad = ctx.createRadialGradient(0,0,radius*0.95, 0,0,radius*1.05);
    grad.addColorStop(0, '#333');
    grad.addColorStop(0.5, 'white');
    grad.addColorStop(1, '#333');
    //define gradient as stroke style
    ctx.strokeStyle = grad;
    ctx.lineWidth = 20;
    //ctx.stroke(); 
}

async function drawTime(ctx, radius){
    var now = new Date();
    var hour = now.getHours();
    var minute = now.getMinutes();
    var second = now.getSeconds();
    //hour
    hour = hour%12;
    //calculate angle of hour hand
    hour = (hour*Math.PI/6)+(minute*Math.PI/(6*60))+(second*Math.PI/(360*60));
    //make hour hand 50% of canvas's radius
    drawHand(ctx, hour, radius*0.6, 30);
    //minute
    //calculate angle of minute hand
    minute=(minute*Math.PI/30)+(second*Math.PI/(30*60));
    //make minute hand 80% of canvas's radius
    drawHand(ctx, minute, radius*0.95, 30);
}

function drawHand(ctx, pos, length, width){
    ctx.beginPath();
    ctx.lineWidth = width;
    ctx.lineCap = "round";
    ctx.moveTo(0,0);
    ctx.rotate(pos);
    ctx.lineTo(0, -length);
    ctx.stroke();
    ctx.rotate(-pos);
}

const stream = new Stream.PassThrough()
await PImage.encodePNGToStream(canvas, stream)
const buffer = await getStream.buffer(stream)
let url = "data:image/png;base64," + buffer.toString("base64");
return url;
}