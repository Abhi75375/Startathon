function StatCards() {
  const stats = [
    {
      label: "Total Spend",
      value: "₹12.4 L",
    },
    {
      label: "Orders",
      value: "24",
    },
    {
      label: "Pending",
      value: "6",
    },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
      {stats.map((stat) => (
        <div
          key={stat.label}
          className="rounded-xl border border-gray-200 bg-white p-5"
        >
          <p className="text-2xl font-semibold">
            {stat.value}
          </p>

          <p className="mt-1 text-sm text-gray-500">
            {stat.label}
          </p>
        </div>
      ))}
    </div>
  );
}

export default StatCards;