const { getUserByOsuId } = require("../api/osu");
const { getTopCounts } = require("../api/osustats");
const { getDiscordUserByDiscordId } = require("../db/users");
const { countPoints } = require("./points");
const { addWysiReaction } = require("./reactions");
const { updateUserData } = require("./userdata");
const { calculatePoints } = require("../messages/counter");

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

module.exports = {
  userLeaderboardsCount
};

