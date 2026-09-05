import { useParams, useNavigate } from "react-router-dom";

const TABS = ["Materials", "Purchase Orders", "Suppliers", "Deliveries"];

function ProjectDetail({ projects }) {
  const { id } = useParams();
  const navigate = useNavigate();
  const project = projects.find((p) => p.id === Number(id));

  if (!project) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-gray-400">
        <p className="text-lg">Project not found.</p>
        <button
          onClick={() => navigate("/projects")}
          className="mt-4 text-sm text-gray-600 underline hover:text-gray-900"
        >
          Back to Projects
        </button>
      </div>
    );
  }

  return (
    <div>
      {/* Back link */}
      <button
        onClick={() => navigate("/projects")}
        className="mb-5 flex items-center gap-1 text-sm text-gray-500 hover:text-gray-900 transition-colors"
      >
        ← Projects
      </button>

      {/* Project name */}
      <h1 className="mb-6 text-2xl font-semibold">{project.name}</h1>

      {/* Budget summary card */}
      <div className="mb-8 inline-block rounded-xl border border-gray-200 bg-white">
        <table className="text-sm">
          <tbody>
            <tr className="border-b border-gray-100">
              <td className="px-6 py-3 text-gray-500">Budget</td>
              <td className="px-6 py-3 font-medium text-gray-900">{project.budget}</td>
            </tr>
            <tr className="border-b border-gray-100">
              <td className="px-6 py-3 text-gray-500">Procured</td>
              <td className="px-6 py-3 font-medium text-gray-900">{project.procured}</td>
            </tr>
            <tr>
              <td className="px-6 py-3 text-gray-500">Remaining</td>
              <td className="px-6 py-3 font-medium text-gray-900">{project.remaining}</td>
            </tr>
          </tbody>
        </table>
      </div>

      {/* Tab placeholders */}
      <div className="space-y-2">
        {TABS.map((tab) => (
          <button
            key={tab}
            className="block w-full rounded-lg border border-gray-200 bg-white px-4 py-3 text-left text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            {tab}
          </button>
        ))}
      </div>
    </div>
  );
}

export default ProjectDetail;
