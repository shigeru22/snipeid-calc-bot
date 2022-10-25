import { calculatePoints } from "./counter";

test("calculatePoints", () => {
  const pointsArray = [ 52, 1892, 3733, 5404, 7496 ];
  expect(calculatePoints(pointsArray[0], pointsArray[1], pointsArray[2], pointsArray[3], pointsArray[4])).toBe(13225);
});
