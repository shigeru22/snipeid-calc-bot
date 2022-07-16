const { greet, agree, disagree, notUnderstood } = require("./msg");

test("greet", () => {
  const messages = [ "Hi", "Hello", "Yes", "What?" ];
  expect(messages).toContain(greet());
});

test("agree", () => {
  const messages = [ "Yea", "Yeah", "Somehow", "It is", "Well yes" ];
  expect(messages).toContain(agree());
});

test("disagree", () => {
  const messages = [ "No", "Nah", "Idk bout that", "Nope" ];
  expect(messages).toContain(disagree());
});

test("notUnderstood", () => {
  const messages = [ "Am I smart enough to understand that?", "What?", "?", "..." ];
  expect(messages).toContain(notUnderstood());
});
