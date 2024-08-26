window.initializeBootstrapDatepicker = (elementId, defaultDate, dotNetHelper) => {
    $(`#${elementId}`).datepicker({
        format: 'mm/dd/yyyy',
        autoclose: true,
        todayHighlight: true
    }).datepicker('update', defaultDate)
        .on('changeDate', function (e) {
            dotNetHelper.invokeMethodAsync('UpdateDate', e.format());
        });
};
