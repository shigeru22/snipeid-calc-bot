import { EmbedBuilder } from "discord.js";

/**
 * Calculates points for each top rank count.
 *
 * @param { number } top1 Top 1 count.
 * @param { number } top8 Top 8 count.
 * @param { number } top15 Top 15 count.
 * @param { number } top25 Top 25 count.
 * @param { number } top50 Top 50 count.
 *
 * @returns { number } Calculated points.
 */
function calculatePoints(top1: number, top8: number, top15: number, top25: number, top50: number): number {
  return top1 * 5 + (top8 - top1) * 3 + (top15 - top8) * 2 + (top25 - top15) + (top50 - top25);
}

/**
 * Calculates points for each top rank count from respektive's API.
 *
 * @param { number } top1 Top 1 count.
 * @param { number } top8 Top 8 count.
 * @param { number } top25 Top 25 count.
 * @param { number } top50 Top 50 count.
 *
 * @returns { number } Calculated points.
 */
function calculateRespektivePoints(top1: number, top8: number, top25: number, top50: number): number {
  return top1 * 5 + (top8 - top1) * 3 + (top25 - top8) + (top50 - top25);
}

/**
 * Creates counter draft embed message.
 *
 * @param { number } top1 Top 1 count.
 * @param { number } top8 Top 8 count.
 * @param { number } top15 Top 15 count.
 * @param { number } top25 Top 25 count.
 * @param { number } top50 Top 50 count.
 * @param { string } username osu! username.
 *
 * @returns { EmbedBuilder } Counter embed data.
 */
function counter(top1: number, top8: number, top15: number, top25: number, top50: number, username: string): EmbedBuilder {
  const points = calculatePoints(top1, top8, top15, top25, top50);

  const draft = new EmbedBuilder().setTitle(`Points for ${ username }:`)
    .setDescription(`\`\`\`${ (top1 * 5).toString().padEnd(6) } = ${ (`${ top1 } x 5`).padStart(19) }\n${ ((top8 - top1) * 3).toString().padEnd(6) } = ${ (`(${ top8 } - ${ top1 }) x 3`).padStart(19) }\n${ ((top15 - top8) * 2).toString().padEnd(6) } = ${ (`(${ top15 } - ${ top8 }) x 2`).padStart(19) }\n${ ((top25 - top15) * 1).toString().padEnd(6) } = ${ (`(${ top25 } - ${ top15 }) x 1`).padStart(19) }\n${ ((top50 - top25) * 1).toString().padEnd(6) } = ${ (`(${ top50 } - ${ top25 }) x 1`).padStart(19) }\n\`\`\`
      = **${ points }** points.`)
    .setColor("#ff0000");

  return draft;
}

/**
 * Creates counter draft embed message.
 *
 * @param { number } top1 Top 1 count.
 * @param { number } top8 Top 8 count.
 * @param { number } top25 Top 25 count.
 * @param { number } top50 Top 50 count.
 * @param { string } username osu! username.
 *
 * @returns { EmbedBuilder } Counter embed data.
 */
function counterRespektive(top1: number, top8: number, top25: number, top50: number, username: string): EmbedBuilder {
  const points = calculateRespektivePoints(top1, top8, top25, top50);

  const draft = new EmbedBuilder().setTitle(`Points for ${ username }:`)
    .setDescription(`\`\`\`\n${ (top1 * 5).toString().padEnd(6) } = ${ (`${ top1 } x 5`).padStart(19) }\n${ ((top8 - top1) * 3).toString().padEnd(6) } = ${ (`(${ top8 } - ${ top1 }) x 3`).padStart(19) }\n${ ((top25 - top8) * 1).toString().padEnd(6) } = ${ (`(${ top25 } - ${ top8 }) x 1`).padStart(19) }\n${ ((top50 - top25) * 1).toString().padEnd(6) } = ${ (`(${ top50 } - ${ top25 }) x 1`).padStart(19) }\n\`\`\`\n= **${ points }** points.`)
    .setFooter({ text: "Note: you might lost points due to respektive's API not returning top 15 count." });

  return draft;
}

export { calculatePoints, calculateRespektivePoints, counter, counterRespektive };
