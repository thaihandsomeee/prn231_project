
function OrdersChart() {
    var xValues = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];

    new Chart("myChart1", {
        type: "line",
        data: {
            labels: xValues,
            datasets: [{
                data: [$("#jan").text()
                    , $("#feb").text()
                    , $("#mar").text()
                    , $("#apr").text()
                    , $("#may").text()
                    , $("#jun").text()
                    , $("#jul").text()
                    , $("#aug").text()
                    , $("#sep").text()
                    , $("#oct").text()
                    , $("#nov").text()
                    , $("#dec").text()
                ],
                borderColor: "sienna",
                fill: true
            }]
        },
        options: {
            legend: { display: false }
        }
    });
}

function CustomersChart() {
    var xValues = ["Total", "New customer"];
    var yValues = [$("#totalCustomer").text(), $("#newCustomer").text() , 50];
    var barColors = ["green", "red"];

    new Chart("myChart2", {
        type: "bar",
        data: {
            labels: xValues,
            datasets: [{
                backgroundColor: barColors,
                data: yValues
            }]
        },
        options: {
            legend: { display: false },
            title: {
                display: true,
                text: "New Customers (30 daily Avg)"
            }
        }
    });
}

OrdersChart();
CustomersChart();