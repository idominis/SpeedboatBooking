window.initializeBootstrapDatepicker = (elementId, defaultDate) => {
    $(`#${elementId}`).datepicker({
        format: 'mm/dd/yyyy',
        autoclose: true,
        todayHighlight: true
    }).datepicker('update', defaultDate);
};
