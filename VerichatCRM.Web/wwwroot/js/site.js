// Shared helpers for the staff web app.

/**
 * The antiforgery token published by _Layout, for JSON POSTs that carry
 * [ValidateAntiForgeryToken] on the server.
 */
function vcRequestToken() {
  return document.querySelector('meta[name="request-verification-token"]')?.content || '';
}
