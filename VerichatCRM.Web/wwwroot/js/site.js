// Shared helpers for the staff web app.

/**
 * The antiforgery token published by _Layout, for JSON POSTs that carry
 * [ValidateAntiForgeryToken] on the server.
 */
function vcRequestToken() {
  return document.querySelector('meta[name="request-verification-token"]')?.content || '';
}

/**
 * The off-canvas navigation, which is the only way to reach the sidebar below
 * 1024px: the theme translates it off screen there. Toggles body.nav-open and
 * keeps the button's own state in step with it, because an aria-expanded that
 * lies is worse to a screen reader than no button at all.
 */
(function vcNavDrawer() {
  const toggle = document.getElementById('vc-nav-toggle');
  const scrim = document.getElementById('vc-nav-scrim');
  const sidebar = document.getElementById('vcSidebar');
  if (!toggle || !scrim || !sidebar) return;

  const isOpen = () => document.body.classList.contains('nav-open');
  const setOpen = (open) => {
    document.body.classList.toggle('nav-open', open);
    toggle.setAttribute('aria-expanded', String(open));
    toggle.setAttribute('aria-label', open ? 'Close navigation' : 'Open navigation');
  };

  toggle.addEventListener('click', () => setOpen(!isOpen()));
  scrim.addEventListener('click', () => setOpen(false));
  document.addEventListener('keydown', (event) => {
    if (event.key === 'Escape' && isOpen()) {
      setOpen(false);
      toggle.focus();
    }
  });
  // On a phone the drawer covers the screen it just navigated to.
  sidebar.addEventListener('click', (event) => {
    if (event.target.closest('a')) setOpen(false);
  });
  window.addEventListener('resize', () => {
    if (window.innerWidth > 1024) setOpen(false);
  });
})();
