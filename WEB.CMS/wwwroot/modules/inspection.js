$(document).ready(function () {
    $('input').attr('autocomplete', 'off');

    _inspection.Init();
    var input_vehicle_number = document.getElementById("input_vehicle_number");
    if (input_vehicle_number != null)
    input_vehicle_number.addEventListener("keyup", function (event) {
        _inspection.getlisst();
    });
    var input_type = document.getElementById("input_type");
    if (input_type != null)
    input_type.addEventListener('change', function () {
         
        _inspection.getlisst();
      });
});
var _inspection = {
    Init: function () {
        _inspection.getlisst();
    },
    ShowAddOrUpdate: function (id) {
        let title = `${id > 0 ? "Cập nhật" : "Thêm mới"} đăng kiểm`;
        let url = '/Inspection/AddOrUpdate';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param);
    },
    OnSave: function () {
        let Form = $('#form_inspection');
        Form.validate({
            rules: {
                "VehicleNumber": "required",
                "VehicleWeight": "required",
                "InspectionDate": "required",
                "ExpirationDate": "required",
            },
            messages: {
                "VehicleNumber": "Vui lòng nhập biển số xe",
                "VehicleWeight": "Vui lòng nhập trọng lượng",
                "InspectionDate": "Vui lòng nhập ngày đăng kiểm",
                "ExpirationDate": "Vui lòng nhập ngày hết hạn đăng kiểm",
            }
        });

        if (!Form.valid()) { return; }


        let formData = {
            Id: $('#Id').val(),
            VehicleNumber: $('#VehicleNumber').val(),
            VehicleWeight: $('#VehicleWeight').val(),
            InspectionDate: $('#InspectionDate').val(),
            ExpirationDate: $('#ExpirationDate').val(),
            VehicleWeightMax: $('#VehicleWeightMax').val(),
        };

        let url = "/Inspection/SetUp";
        $.ajax({
            url: url,
            type: "POST",
            data: { model: formData },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    setTimeout(function () {
                        window.location.reload()
                    }, 1000);
                } else {
                    _msgalert.error(result.msg);
                }
            }
        });
    },
    getlisst: function () {

        var model = {
            VehicleNumber: $('#input_vehicle_number').val(),
            type: $('#input_type').val(), 

        }
        $.ajax({
            url: "/Inspection/GetListInspection",
            type: "post",
            data: { searchModel: model },
            success: function (result) {
                $('#grid_data').html(result)
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    }
}