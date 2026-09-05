import {
  LayoutDashboard,
  FolderKanban,
  Truck,
  Package,
  ShoppingCart,
  ClipboardList,
  BarChart3,
  Settings,
} from "lucide-react";

function Sidebar() {
  const mainLinks = [
    {
      label: "Overview",
      icon: LayoutDashboard,
    },
    {
      label: "Projects",
      icon: FolderKanban,
    },
    {
      label: "Suppliers",
      icon: Truck,
    },
    {
      label: "Materials",
      icon: Package,
    },
    {
      label: "Orders",
      icon: ShoppingCart,
    },
    {
      label: "Requests",
      icon: ClipboardList,
    },
  ];

  const secondaryLinks = [
    {
      label: "Reports",
      icon: BarChart3,
    },
    {
      label: "Settings",
      icon: Settings,
    },
  ];

  const renderLink = (link, active = false) => {
    const Icon = link.icon;

    return (
      <button
        key={link.label}
        className={`flex w-full items-center gap-3 rounded-lg px-3 py-2.5 text-sm transition ${
          active
            ? "bg-gray-100 text-gray-900"
            : "text-gray-500 hover:bg-gray-50 hover:text-gray-900"
        }`}
      >
        <Icon size={18} strokeWidth={1.8} />

        <span>{link.label}</span>
      </button>
    );
  };

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 border-r border-gray-200 bg-white">
      {/* Brand */}
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <span className="text-lg font-semibold tracking-tight">
          Procurement
        </span>
      </div>

      {/* Navigation */}
      <nav className="p-4">
        <div className="space-y-1">
          {mainLinks.map((link) =>
            renderLink(link, link.label === "Overview")
          )}
        </div>

        <div className="my-5 border-t border-gray-200" />

        <div className="space-y-1">
          {secondaryLinks.map((link) => renderLink(link))}
        </div>
      </nav>
    </aside>
  );
}

export default Sidebar;