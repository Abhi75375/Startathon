import { NavLink } from "react-router-dom";

const mainLinks = [
  { label: "Overview", to: "/overview" },
  { label: "Projects", to: "/projects" },
  { label: "Suppliers", to: "/suppliers" },
  { label: "Materials", to: "/materials" },
  { label: "Orders", to: "/orders" },
  { label: "Requests", to: "/requests" },
];

const secondaryLinks = [
  { label: "Reports", to: "/reports" },
  { label: "Settings", to: "/settings" },
];

function Sidebar() {
  const linkClass = ({ isActive }) =>
    `block w-full rounded-lg px-3 py-2 text-left text-sm transition-colors ${
      isActive
        ? "bg-gray-100 font-medium text-gray-900"
        : "text-gray-600 hover:bg-gray-50"
    }`;

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 border-r border-gray-200 bg-white">
      {/* Brand */}
      <div className="flex h-16 items-center border-b border-gray-200 px-6">
        <span className="text-lg font-semibold">Procurement</span>
      </div>

      {/* Navigation */}
      <nav className="p-4">
        <div className="space-y-1">
          {mainLinks.map(({ label, to }) => (
            <NavLink key={to} to={to} className={linkClass}>
              {label}
            </NavLink>
          ))}
        </div>

        <div className="my-5 border-t border-gray-200" />

        <div className="space-y-1">
          {secondaryLinks.map(({ label, to }) => (
            <NavLink key={to} to={to} className={linkClass}>
              {label}
            </NavLink>
          ))}
        </div>
      </nav>
    </aside>
  );
}

export default Sidebar;
