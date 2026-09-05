import Sidebar from "./components/Sidebar";
import Navbar from "./components/Navbar";
import StatCards from "./components/StatCards";
import RecentOrders from "./components/RecentOrders";
import SpendOverview from "./components/SpendOverview";
import PendingRequests from "./components/PendingRequests";

function App() {
  return (
    <div className="min-h-screen bg-gray-50 text-gray-900">
      <Sidebar />

      <div className="ml-64">
        <Navbar />

        <main className="p-8">
          <h1 className="mb-7 text-2xl font-semibold tracking-tight">Dashboard</h1>
        </main>
        <StatCards />
        <RecentOrders />
        <div className="mt-6 grid grid-cols-1 gap-6 lg:grid-cols-[2fr_1fr]">
          <SpendOverview />
          <PendingRequests />
        </div>
      </div>
    </div>
  );
}

export default App;