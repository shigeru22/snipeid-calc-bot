function parseTopCountDescription(desc) {
  const res = desc.replace(/```(\n)*/g, "");
  const tops = res.split("\n");

  const len = tops.length;
  for(let i = 0; i < len; i++) {
    const temp = tops[i];
    
    tops[i] = temp.replace(/Top /g, "");
    tops[i] = tops[i].replace(/ /g, "");
    tops[i] = tops[i].replace(/,/g, "");
  }

  const topsArray = [];
  tops.forEach(item => {
    if(item !== "") {
      const temp = item.split(":");
      topsArray.push(parseInt(temp[1], 10));
    }
  });

  return topsArray;
}

function parseUsername(title) {
  const res = title.replace("In how many top X map leaderboards is ", "")
    .replace("?", "");  
  return res;
}

module.exports = {
  parseTopCountDescription,
  parseUsername
};
