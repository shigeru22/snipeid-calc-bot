/**
 * Returns greet message.
 *
 * @returns { string }
 */
function greet() {
  const messages = [ "Hi", "Hello", "Yes", "What?" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns agree message.
 *
 * @returns { string }
 */
function agree() {
  const messages = [ "Yea", "Yeah", "Somehow", "It is", "Well yes" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns disagree message.
 *
 * @returns { string }
 */
function disagree() {
  const messages = [ "No", "Nah", "Idk bout that", "Nope" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns not understood message.
 *
 * @returns { string }
 */
function notUnderstood() {
  const messages = [ "Am I smart enough to understand that?", "What?", "?", "..." ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

module.exports = {
  greet,
  agree,
  disagree,
  notUnderstood
};
