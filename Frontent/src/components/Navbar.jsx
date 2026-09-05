function Navbar() {
  return (
    <header className="flex h-16 items-center justify-end border-b border-gray-200 bg-white px-6">
      <div className="flex items-center gap-5">
        {/* Notification */}
        <button className="text-gray-500 hover:text-gray-900">
          🔔
        </button>

        {/* Admin */}
        <button className="flex items-center gap-2 text-sm font-medium">
          Admin
          <span className="text-xs text-gray-500">▾</span>
        </button>
      </div>
    </header>
  );
}

export default Navbar;