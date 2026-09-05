import Sidebar from "./components/Sidebar";
import Navbar from "./components/Navbar";

function App() {
  return (
    <div className="min-h-screen bg-gray-50 text-gray-900">
      <Sidebar />

      <div className="ml-64">
        <Navbar />

        <main className="p-6">
          <h1 className="text-2xl font-semibold">Dashboard</h1>
        </main>
      </div>
    </div>
  );
}

export default App;