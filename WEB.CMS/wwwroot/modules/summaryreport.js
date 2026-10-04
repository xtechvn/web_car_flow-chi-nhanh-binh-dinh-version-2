$(document).ready(function () {
    
    _summary_report.init();
});
let revenuChartInstance = null;
let revenuChartInstance_KhungGio = null;
let revenuChartInstance_mang = null;
var _summary_report = {
    init: function () {
        var date = new Date();
        var datetime = new Date(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0).toLocaleDateString("en-GB");
           
       /* _summary_report.GetDailyStatistics(datetime)*/
        _summary_report.GetTotalWeightByHour(datetime);
        _summary_report.GetProductivityStatistics(datetime);
        _summary_report.GetTotalWeightByTroughType(datetime);
        _summary_report.TotalVehicleInspection(datetime);
        _summary_report.GetTotalWeightByWeightGroup(datetime);
    },
    Seach: function () {
        var text = $('#date_time_Car').val();
        parse_value = text.split(' ')[0].split('-')
        var datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
       /* _summary_report.GetDailyStatistics(datetime)*/
        _summary_report.GetTotalWeightByHour(datetime);
        _summary_report.GetTotalWeightByWeightGroup(datetime);
        _summary_report.GetProductivityStatistics(datetime);
        _summary_report.TotalVehicleInspection(datetime);

    },
    Seach_mang: function () {
        var text = $('#date_time_mang').val();
        parse_value = text.split(' ')[0].split('-')
        var datetime = parse_value[2] + '/' + parse_value[1] + '/' + parse_value[0];
        _summary_report.GetTotalWeightByTroughType(datetime);
    },

    GetDailyStatistics: function (datetime) {
        $.ajax({
            url: "/SummaryReport/DailyStatistics",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                $('#Grid-DailyStatistics').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    GetProductivityStatistics: function (datetime) {
        $.ajax({
            url: "/SummaryReport/GetProductivityStatistics",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                $('#Grid-ProductivityStatistics').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    TotalVehicleInspection: function (datetime) {
        $.ajax({
            url: "/SummaryReport/TotalVehicleInspection",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                $('#data_Total').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    GetTotalWeightByHour: function (datetime) {
        $.ajax({
            url: "/SummaryReport/GetTotalWeightByHour",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                if (result.isSuccess != false) {
                    
                    result.data.totalWeightInHour
                    _summary_report.Bieudo_KhungGio(result.data.khungGio, result.data.sanLuong,
                        result.data.tan_Moi_Gio);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    GetTotalWeightByWeightGroup: function (datetime) {
        $.ajax({
            url: "/SummaryReport/GetTotalWeightByWeightGroup",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                if (result.isSuccess != false) {
                    
                    result.data.totalWeightInHour
                    _summary_report.Bieudo(result.data.weightGroup, result.data.sanLuong,
                        result.data.soPhut_Tren_Tan, result.data.soPhut_Tren_Xe);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    Bieudo: function (listLabel, nangSuatLoadingData, SoPhut_Tren_Tan, SoPhut_Tren_Xe) {

        if (window.revenuChartInstance) {
            window.revenuChartInstance.destroy();
        }

        var canvas = document.getElementById("revenuChart");
        var ctx = canvas.getContext('2d');

        var barChartData = {
            labels: listLabel,
            datasets: [
                {
                    label: 'Sản lượng (tấn)',
                    data: nangSuatLoadingData,
                    backgroundColor: '#00e6ff',
                    borderColor: '#00e6ff',
                    borderWidth: 1,
                    barPercentage: 0.25
                },
                {
                    label: 'Phút/Tấn',
                    data: SoPhut_Tren_Tan,
                    backgroundColor: '#ffff00',
                    borderColor: '#ffff00',
                    borderWidth: 1,
                    barPercentage: 0.25
                },
                {
                    label: 'Phút/Xe',
                    data: SoPhut_Tren_Xe,
                    backgroundColor: '#2cb161',
                    borderColor: '#2cb161',
                    borderWidth: 1,
                    barPercentage: 0.25
                }
            ]
        };

        var ObjectChart = {
            type: 'bar',
            data: barChartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                tooltips: { enabled: false },
                hover: { mode: null, onHover: null },
                events: [],

                legend: {
                    labels: { fontColor: '#000' },
                    onClick: function () { }
                },

                title: {
                    display: true,
                    text: 'Xu hướng khối lượng xuất và năng suất loading',
                    fontColor: '#000',
                    fontSize: 16
                },

                scales: {
                    xAxes: [{
                        display: true,
                        ticks: { fontColor: '#000' },
                        scaleLabel: {
                            display: true,
                            labelString: '',
                            fontColor: '#000'
                        },
                        gridLines: { display: false },
                        barPercentage: 0.9,
                        categoryPercentage: 0.7
                    }],
                    yAxes: [{
                        display: false, // 💥 Ẩn hoàn toàn trục Y
                        gridLines: { display: false },
                        ticks: { display: false }
                    }]
                },

                animation: {
                    duration: 0,
                    onComplete: function () {
                        var chartInstance = this.chart;
                        var ctx = chartInstance.ctx;
                        ctx.textAlign = 'center';
                        ctx.textBaseline = 'middle';
                        ctx.font = 'bold 11px Arial';
                        ctx.fillStyle = '#000';

                        this.data.datasets.forEach(function (dataset, i) {
                            var meta = chartInstance.controller.getDatasetMeta(i);
                            meta.data.forEach(function (bar, index) {
                                var value = dataset.data[index];
                                if (value != null) {
                                    ctx.fillText(value, bar._model.x, bar._model.y - 10);
                                }
                            });
                        });
                    }
                }
            }
        };

        window.revenuChartInstance = new Chart(ctx, ObjectChart);
        if (window.revenuChartInstance) window.revenuChartInstance.update();
    }
    ,
    GetTotalWeightByTroughType: function (datetime) {
        $.ajax({
            url: "/SummaryReport/TotalWeightByTroughType",
            type: "post",
            data: { date: datetime },
            success: function (result) {
                if (result.isSuccess != false) {
                    _summary_report.Bieudo_mang(result.data.troughType, result.data.sanLuong,
                        result.data.tongGio, result.data.tan_Moi_Gio);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    Bieudo_mang: function (listLabel, nangSuatLoadingData, SoPhut_Tren_Tan, SoPhut_Tren_Xe) {

        if (window.revenuChartInstance_mang) {
            window.revenuChartInstance_mang.destroy();
        }

        var canvas = document.getElementById("revenuChart_mang");
        var ctx = canvas.getContext('2d');

        var barChartData = {
            labels: listLabel,
            datasets: [
                {
                    label: 'Sản lượng (Tấn)',
                    data: nangSuatLoadingData,
                    backgroundColor: '#00e6ff',
                    borderColor: '#00e6ff',
                    borderWidth: 1,
                    barPercentage: 0.25
                },
                {
                    label: 'Tổng thời gian (Giờ)',
                    data: SoPhut_Tren_Tan,
                    backgroundColor: '#ffff00',
                    borderColor: '#ffff00',
                    borderWidth: 1,
                    barPercentage: 0.25
                },
                {
                    label:'Năng xuất (Tấn/Giờ)',
                    data: SoPhut_Tren_Xe,
                    backgroundColor: '#2cb161',
                    borderColor: '#2cb161',
                    borderWidth: 1,
                    barPercentage: 0.25
                }
            ]
        };

        var ObjectChart = {
            type: 'bar',
            data: barChartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                tooltips: { enabled: false },
                hover: { mode: null, onHover: null },
                events: [],

                legend: {
                    labels: { fontColor: '#000' },
                    onClick: function () { }
                },

                title: {
                    display: true,
                    text: '',
                    fontColor: '#000',
                    fontSize: 16
                },

                scales: {
                    xAxes: [{
                        display: true,
                        ticks: { fontColor: '#000' },
                        scaleLabel: {
                            display: true,
                            labelString: '',
                            fontColor: '#000'
                        },
                        gridLines: { display: false },
                        barPercentage: 0.9,
                        categoryPercentage: 0.7
                    }],
                    yAxes: [{
                        display: false, // 💥 Ẩn hoàn toàn trục Y
                        gridLines: { display: false },
                        ticks: { display: false }
                    }]
                },

                animation: {
                    duration: 0,
                    onComplete: function () {
                        var chartInstance = this.chart;
                        var ctx = chartInstance.ctx;
                        ctx.textAlign = 'center';
                        ctx.textBaseline = 'middle';
                        ctx.font = 'bold 11px Arial';
                        ctx.fillStyle = '#000';

                        this.data.datasets.forEach(function (dataset, i) {
                            var meta = chartInstance.controller.getDatasetMeta(i);
                            meta.data.forEach(function (bar, index) {
                                var value = dataset.data[index];
                                if (value != null) {
                                    ctx.fillText(value, bar._model.x, bar._model.y - 10);
                                }
                            });
                        });
                    }
                }
            }
        };

        window.revenuChartInstance_mang = new Chart(ctx, ObjectChart);
        if (window.revenuChartInstance_mang) window.revenuChartInstance_mang.update();
    },
    Bieudo_KhungGio: function (listLabel, volXuatData, nangSuatLoadingData) {
        //const listLabel = ['1', '2', '3', '4', '5', '7', '8', '9', '14', '15', '16', '17', '18', '19', '21', '22', '23', '24'];
        //const volXuatData = [159, 71, 74, 146, 151, 185, 110, 163, 159, 179, 90, 141, 230, 132, 175, 50, 137, 50];
        //const nangSuatLoadingData = [159, 71, 74, 146, 151, 185, 110, 163, 159, 179, 90, 141, 230, 132, 175, 50, 137, 50];

        if (window.revenuChartInstance_KhungGio) {
            window.revenuChartInstance_KhungGio.destroy();
        }

        var canvas = document.getElementById("revenuChart_KhungGio");
        var ctx = canvas.getContext('2d');

        var barChartData = {
            labels: listLabel,
            datasets: [
                {
                    label: 'Vol. xuất (tấn)',
                    data: volXuatData,
                    backgroundColor: 'rgba(10, 140, 50, 0.8)',
                    borderColor: 'rgba(10, 140, 50, 1)',

                    yAxisID: 'y-axis-0',

                },
                {
                    label: 'Năng suất loading (tấn/h)',
                    data: nangSuatLoadingData,
                    type: 'line',
                    borderColor: '#2f3db5',
                    backgroundColor: 'rgba(54, 162, 235, 0.1)',
                    yAxisID: 'y-axis-1',


                }
            ]
        };

        var ObjectChart = {
            type: 'bar',
            data: barChartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,

                // ---- CHỐT: TẮT HOÀN TOÀN TOOLTIP & HOVER ----
                // 1) tắt tooltip built-in
                tooltips: { enabled: false },
                // 2) tắt hover mode
                hover: { mode: null, onHover: null },
                // 3) KHÓA hoàn toàn event chuột (tắt mọi event mouse)
                events: [],

                // nếu bạn dùng legend click để show/hide, có thể disable onClick:
                legend: {
                    onClick: function () { /* no-op */ }
                },

                scales: {
                    xAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Mốc thời gian'
                        },
                        gridLines: { display: false }
                    }],
                    yAxes: [{
                        id: 'y-axis-0',
                        type: 'linear',
                        position: 'left',
                        stacked: true,
                        ticks: {
                            beginAtZero: true,
                            stepSize: 20,
                            callback: function (value) { return value + ' Tấn'; }
                        },
                        gridLines: { color: '#f1f1f1' }
                    }, {
                        id: 'y-axis-1',
                        type: 'linear',
                        position: 'right',
                        gridLines: { drawOnChartArea: false },
                        ticks: { display: false, beginAtZero: true, stepSize: 20, },

                    }]
                },

                animation: {
                    duration: 0,
                    onComplete: function () {
                        var chartInstance = this.chart;
                        var ctx = chartInstance.ctx;
                        ctx.textAlign = 'center';
                        ctx.textBaseline = 'middle';
                        ctx.font = 'bold 11px Arial';

                        this.data.datasets.forEach(function (dataset, i) {
                            var meta = chartInstance.controller.getDatasetMeta(i);
                            meta.data.forEach(function (bar, index) {
                                var value = dataset.data[index];

                                if (dataset.type === 'line') {
                                    ctx.fillStyle = 'rgba(54, 162, 235, 1)';
                                    ctx.fillText(value, bar._model.x, bar._model.y - 10);
                                } else {
                                    const barHeight = Math.abs(bar._model.y - bar._yScale.bottom);
                                    const textY = bar._model.y + barHeight / 2;
                                    ctx.fillStyle = (barHeight > 40) ? '#ffffff' : '#000000';
                                    ctx.fillText(value, bar._model.x, textY);
                                }
                            });
                        });
                    }
                },

                title: {
                    display: true,
                    text: 'Sản lượng theo giờ'
                }
            }
        };

        // create chart
        window.revenuChartInstance_KhungGio = new Chart(ctx, ObjectChart);

        // đảm bảo cập nhật
        if (window.revenuChartInstance_KhungGio) window.revenuChartInstance_KhungGio.update();
    }
}