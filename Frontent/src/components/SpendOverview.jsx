function SpendOverview() {
  return (
    <section className="rounded-xl border border-gray-200 bg-white">
      <div className="border-b border-gray-200 px-5 py-4">
        <h2 className="text-base font-semibold">
          Spend Overview
        </h2>
      </div>

      <div className="flex h-56 items-end px-6 pb-6 pt-4">
        <div className="flex h-full w-full items-end gap-3">
          {[35, 55, 45, 70, 60, 85, 65, 90, 75, 95, 80, 100].map(
            (height, index) => (
              <div
                key={index}
                className="flex-1 rounded-t bg-gray-200"
                style={{ height: `${height}%` }}
              />
            )
          )}
        </div>
      </div>
    </section>
  );
}

export default SpendOverview;