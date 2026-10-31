type RefreshListener = () => void;

const listeners = new Set<RefreshListener>();

export function subscribeGlobalRefresh(listener: RefreshListener) {
  listeners.add(listener);
  return () => { listeners.delete(listener); };
}

export function emitGlobalRefresh() {
  listeners.forEach((listener) => listener());
}
