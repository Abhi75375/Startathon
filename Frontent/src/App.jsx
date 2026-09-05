import { useState } from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import Sidebar from "./components/Sidebar";
import Navbar from "./components/Navbar";
import Overview from "./pages/Overview";
import Projects from "./pages/Projects";
import ProjectDetail from "./pages/ProjectDetail";

const INITIAL_PROJECTS = [
  { id: 1, name: "Green Heights", location: "Kochi", budget: "₹45L", procured: "₹28L", remaining: "₹17L", status: "Active" },
  { id: 2, name: "Metro Tower", location: "Trivandrum", budget: "₹82L", procured: "₹55L", remaining: "₹27L", status: "Active" },
  { id: 3, name: "Villa Project", location: "Kollam", budget: "₹22L", procured: "₹22L", remaining: "₹0L", status: "Completed" },
];

function App() {
  const [projects, setProjects] = useState(INITIAL_PROJECTS);

  function handleAddProject(project) {
    setProjects((prev) => [...prev, project]);
  }

  return (
    <BrowserRouter>
      <div className="min-h-screen bg-gray-50 text-gray-900">
        <Sidebar />

        <div className="ml-64">
          <Navbar />

          <main className="p-6">
            <Routes>
              <Route path="/" element={<Navigate to="/overview" replace />} />
              <Route path="/overview" element={<Overview />} />
              <Route
                path="/projects"
                element={
                  <Projects projects={projects} onAdd={handleAddProject} />
                }
              />
              <Route
                path="/projects/:id"
                element={<ProjectDetail projects={projects} />}
              />
            </Routes>
          </main>
        </div>
      </div>
    </BrowserRouter>
  );
}

export default App;
