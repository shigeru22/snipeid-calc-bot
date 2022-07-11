const { getUserByOsuId } = require("../api/osu");
const { getTopCounts } = require("../api/osustats");
const { getDiscordUserByDiscordId } = require("../db/users");
const { countPoints } = require("./points");
const { addWysiReaction } = require("./reactions");
const { updateUserData } = require("./userdata");
const { calculatePoints } = require("../messages/counter");

// TODO: implement type checks
// TODO: add user check

async function userLeaderboardsCount(client, channel, db, osuToken, discordId) {
  const user = await getDiscordUserByDiscordId(db, discordId);
  const osuUser = await getUserByOsuId(osuToken, user.osuId);

  let topCounts = [];
  {
    const topCountsRequest = [
      getTopCounts(osuUser.username, 1),
      getTopCounts(osuUser.username, 8),
      getTopCounts(osuUser.username, 15),
      getTopCounts(osuUser.username, 25),
      getTopCounts(osuUser.username, 50)
    ];
    const temp = await Promise.all(topCountsRequest);
    topCounts = [ temp[0].count, temp[1].count, temp[2].count, temp[3].count, temp[4].count ];
  }

  const points = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);
  const message = await countPoints(channel, osuUser.username, topCounts);
  await addWysiReaction(client, message, topCounts, points);

  await updateUserData(osuToken, client, channel, db, user.osuId, points); 
}

async function userWhatIfCount(client, channel, db, osuToken, discordId, whatIfsArray) {
  const user = await getDiscordUserByDiscordId(db, discordId);
  const osuUser = await getUserByOsuId(osuToken, user.osuId);

  let topCounts = [];
  {
    const topCountsRequest = [
      getTopCounts(osuUser.username, 1),
      getTopCounts(osuUser.username, 8),
      getTopCounts(osuUser.username, 15),
      getTopCounts(osuUser.username, 25),
      getTopCounts(osuUser.username, 50)
    ];
    const temp = await Promise.all(topCountsRequest);
    topCounts = [ temp[0].count, temp[1].count, temp[2].count, temp[3].count, temp[4].count ];
  }

  const originalPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

  const tops = [ 1, 8, 15, 25, 50 ];

  whatIfsArray.forEach(whatif => {
    const topIndex = tops.findIndex(top => top === whatif[0]);
    // TODO: handle index not found
    
    topCounts[topIndex] = whatif[1];
  });

  const newPoints = calculatePoints(topCounts[0], topCounts[1], topCounts[2], topCounts[3], topCounts[4]);

  console.log(topCounts);
  const message = await countPoints(channel, osuUser.username, topCounts);
  await addWysiReaction(client, message, topCounts, newPoints);

  const difference = newPoints - originalPoints;
  if(difference === 0) {
    await channel.send(`<@${ discordId }> would increase nothing!`);
    return;
  }

  await channel.send(`<@${ discordId }> would **${ difference > 0 ? "increase" : "decrease" } ${ Math.abs(difference) }** points from current top count.`);
}

module.exports = {
  userLeaderboardsCount,
  userWhatIfCount
};

