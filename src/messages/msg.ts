/**
 * Returns greet message.
 *
 * @returns { string } The message.
 */
function greet(): string {
  const messages = [ "Hi", "Hello", "Yes", "What?" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns agree message.
 *
 * @returns { string } The message.
 */
function agree(): string {
  const messages = [ "Yea", "Yeah", "Somehow", "It is", "Well yes" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns disagree message.
 *
 * @returns { string } The message.
 */
function disagree(): string {
  const messages = [ "No", "Nah", "Idk bout that", "Nope" ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

/**
 * Returns not understood message.
 *
 * @returns { string } The message.
 */
function notUnderstood(): string {
  const messages = [ "Am I smart enough to understand that?", "What?", "?", "..." ];
  const index = Math.floor(Math.random() * (messages.length - 0.1));
  return messages[index];
}

export { greet, agree, disagree, notUnderstood };
