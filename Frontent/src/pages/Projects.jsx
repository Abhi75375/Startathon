import { useState } from "react";
import { useNavigate } from "react-router-dom";
import AddProjectModal from "../components/AddProjectModal";

const STATUS_STYLES = {
  Active: "bg-green-100 text-green-700",
  Completed: "bg-gray-100 text-gray-600",
  "On Hold": "bg-yellow-100 text-yellow-700",
};

function Projects({ projects, onAdd }) {
  const [showModal, setShowModal] = useState(false);
  const navigate = useNavigate();

  return (
    <div>
      {/* Page header */}
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Projects</h1>
        <button
          onClick={() => setShowModal(true)}
          className="flex items-center gap-2 rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-700 transition-colors"
        >
          <span className="text-base leading-none">+</span>
          Add Project
        </button>
      </div>

      {/* Table */}
      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-200 bg-gray-50">
              <th className="px-4 py-3 text-left font-medium text-gray-500">Project Name</th>
              <th className="px-4 py-3 text-left font-medium text-gray-500">Location</th>
              <th className="px-4 py-3 text-left font-medium text-gray-500">Budget</th>
              <th className="px-4 py-3 text-left font-medium text-gray-500">Status</th>
            </tr>
          </thead>
          <tbody>
            {projects.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-4 py-8 text-center text-gray-400">
                  No projects yet. Click &ldquo;+ Add Project&rdquo; to create one.
                </td>
              </tr>
            ) : (
              projects.map((project, idx) => (
                <tr
                  key={project.id}
                  onClick={() => navigate(`/projects/${project.id}`)}
                  className={`cursor-pointer transition-colors hover:bg-gray-50 ${
                    idx !== projects.length - 1 ? "border-b border-gray-100" : ""
                  }`}
                >
                  <td className="px-4 py-3 font-medium text-gray-900">{project.name}</td>
                  <td className="px-4 py-3 text-gray-600">{project.location}</td>
                  <td className="px-4 py-3 text-gray-600">{project.budget}</td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-block rounded-full px-2.5 py-0.5 text-xs font-medium ${
                        STATUS_STYLES[project.status] ?? "bg-gray-100 text-gray-600"
                      }`}
                    >
                      {project.status}
                    </span>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      {showModal && (
        <AddProjectModal
          onClose={() => setShowModal(false)}
          onAdd={onAdd}
        />
      )}
    </div>
  );
}

export default Projects;
