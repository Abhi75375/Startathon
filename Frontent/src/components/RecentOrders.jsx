function RecentOrders() {
  const orders = [
    {
      id: "PO-1024",
      material: "Cement",
      amount: "₹82,000",
      status: "Pending",
    },
    {
      id: "PO-1023",
      material: "Steel",
      amount: "₹1.2 L",
      status: "Approved",
    },
    {
      id: "PO-1022",
      material: "Electrical",
      amount: "₹45,000",
      status: "Delivered",
    },
    {
      id: "PO-1021",
      material: "Tiles",
      amount: "₹68,000",
      status: "Pending",
    },
  ];

  return (
    <section className="mt-6 rounded-xl border border-gray-200 bg-white">
      {/* Header */}
      <div className="border-b border-gray-200 px-5 py-4">
        <h2 className="text-base font-semibold">
          Recent Orders
        </h2>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-100 text-left text-gray-500">
              <th className="px-5 py-3 font-medium">
                Order
              </th>

              <th className="px-5 py-3 font-medium">
                Material
              </th>

              <th className="px-5 py-3 font-medium">
                Amount
              </th>

              <th className="px-5 py-3 font-medium">
                Status
              </th>
            </tr>
          </thead>

          <tbody>
            {orders.map((order) => (
              <tr
                key={order.id}
                className="border-b border-gray-100 last:border-0 hover:bg-gray-50"
              >
                <td className="px-5 py-4 font-medium">
                  {order.id}
                </td>

                <td className="px-5 py-4 text-gray-600">
                  {order.material}
                </td>

                <td className="px-5 py-4 text-gray-600">
                  {order.amount}
                </td>

                <td className="px-5 py-4">
                  <span
                    className={`rounded-full px-2.5 py-1 text-xs font-medium ${
                      order.status === "Pending"
                        ? "bg-yellow-50 text-yellow-700"
                        : order.status === "Approved"
                        ? "bg-blue-50 text-blue-700"
                        : "bg-green-50 text-green-700"
                    }`}
                  >
                    {order.status}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

export default RecentOrders;