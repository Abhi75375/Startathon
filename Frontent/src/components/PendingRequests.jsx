function PendingRequests() {
  return (
    <section className="rounded-xl border border-gray-200 bg-white">
      <div className="border-b border-gray-200 px-5 py-4">
        <h2 className="text-base font-semibold">
          Pending Requests
        </h2>
      </div>

      <div className="space-y-5 p-5">
        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">
            Approvals
          </span>

          <span className="text-lg font-semibold">
            3
          </span>
        </div>

        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-600">
            Quotations
          </span>

          <span className="text-lg font-semibold">
            2
          </span>
        </div>

        <button className="mt-2 w-full rounded-lg border border-gray-200 px-4 py-2 text-sm font-medium hover:bg-gray-50">
          View Requests
        </button>
      </div>
    </section>
  );
}

export default PendingRequests;