function Sidebar() {
  const mainLinks = [
    "Overview",
    "Projects",
    "Suppliers",
    "Materials",
    "Orders",
    "Requests",
  ];

  const secondaryLinks = [
    "Reports",
    "Settings",
  ];

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 border-r border-gray-200 bg-white">
      {/* Brand */}
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <span className="text-lg font-semibold">
          Procurement
        </span>
      </div>

      {/* Navigation */}
      <nav className="p-4">
        <div className="space-y-1">
          {mainLinks.map((link) => (
            <button
              key={link}
              className={`w-full rounded-lg px-3 py-2 text-left text-sm ${
                link === "Overview"
                  ? "bg-gray-100 font-medium"
                  : "text-gray-600 hover:bg-gray-50"
              }`}
            >
              {link}
            </button>
          ))}
        </div>

        <div className="my-5 border-t border-gray-200" />

        <div className="space-y-1">
          {secondaryLinks.map((link) => (
            <button
              key={link}
              className="w-full rounded-lg px-3 py-2 text-left text-sm text-gray-600 hover:bg-gray-50"
            >
              {link}
            </button>
          ))}
        </div>
      </nav>
    </aside>
  );
}

export default Sidebar;