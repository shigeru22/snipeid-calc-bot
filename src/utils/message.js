function greet() {
  const messages = [ "Hi", "Hello", "Yes", "What?" ];
  const index = Math.floor(Math.random() * 3.9);
  return messages[index];
}

function agree() {
  const messages = [ "Yea", "Yeah", "Somehow", "It is", "Well yes" ];
  const index = Math.floor(Math.random() * 4.9);
  return messages[index];
}

function disagree() {
  const messages = [ "No", "Nah", "Idk bout that", "Nope" ];
  const index = Math.floor(Math.random() * 3.9);
  return messages[index];
}

function notUnderstood() {
  const messages = [ "Am I smart enough to understand that?", "What?", "?", "..." ];
  const index = Math.floor(Math.random() * 3.9);
  return messages[index];
}

module.exports = {
  greet,
  agree,
  disagree,
  notUnderstood
};
