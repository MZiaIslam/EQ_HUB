$(document).keypress(function (e) {
    if (e.keyCode === 13) {
        e.preventDefault();
        return false;
    }
});
function FormatNum(num) {
    var p = num.toString().split(".");
    var d = "";
    if (p[1] != undefined) {
        d = "." + p[1];
    }
    return "" + p[0].split("").reverse().reduce(function (acc, num, i, orig) {
        return num + (i && !(i % 3) ? "," : "") + acc;
    }, "") + d;
}
$(document).ready(function () {
    $("#dtlD").on('click', ".DelSec", function (e) {
        var r = confirm("Are you sure want to delete selected report section?");
        var me = $(this);
        if (r == true) {
            $.ajax({
                url: "DelSection?ID=" + me.attr("tag"),
                context: document.body,
                success: function () {
                    $('#t' + me.attr('tag')).parent().hide();
                }
            });
        }
    });
    $(document).on('focusout', ".tRpt", function (e) {
   
        var me = $(this);

            $.ajax({
                url: "updatetxt?ID=" + me.attr("id")+"&txt="+me.html(),
                context: document.body,
                success: function () {
                    //$('#t' + me.attr('tag')).parent().hide();
                }
            });
       
    });
    $("#dtlD").on('change', ".btnColSize", function (e) {
        var me = $(this);
        $.ajax({
            url: "SetColSize?ID=" + me.attr("tag") + "&size=" + me.val(),
            context: document.body,
            success: function () {
                $('#t' + me.attr('tag')).parent().attr("class", "dtlD " + me.val());
                $(window).trigger('resize');
            }
        });
    });
    $("#btndel").on('click', function (e) {
        var r = confirm("Are you sure want to delete this report?");
        if (r == true) {
            $.ajax({
                url: "Reporting.ashx/?ID=4:" + getCookie("rptID"),
                context: document.body,
                success: function (responseText) { location.href = '/MyReports'; }
            });
        }
    });
    $("#btnsv").on('click', function (e) {
        $(".dtlD").each(function (index, value) {
            var cC = $('.chrt-tb' + ($(this).children().attr('id').substring(1)));
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "ReportView.aspx/upSize",
                data: '{"height":"' + cC.height() + '","width":"' + cC.width() + '","RptID":"' + $(this).find('.chrt').attr('id').substring(1) + '"}',
                context: document.body,
                success: function (responseText) {
                }
            });
        });
        $(".chrt").each(function (index, value) {
            $.ajax({
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: "ReportView.aspx/UpdateRpt",
                data: '{"sort":"' + index + '","remarks":"' + encodeURIComponent($('#t' + $(this).attr('id').substring(1)).html()) + '","RptID":"' + $(this).attr('id').substring(1) + '"}',
                context: document.body,
                success: function (responseText) {
                }
            });
        });
    });
    $("#btndash").on('click', function (e) {
        var atcS;
        if ($(this).attr('bit') == "0") { atcS = 1; $(this).removeClass('btn-default'); $(this).addClass('btn-success'); $(this).attr('bit', '1'); } else { atcS = 0; $(this).removeClass('btn-success'); $(this).addClass('btn-default'); $(this).attr('bit', '0'); }
        $.ajax({
            url: "SetDash?id=" + $(this).attr('bit') + "&act=" + atcS,
            context: document.body,
            success: function (responseText) {
            }
        });
    });
    $("#btnArchives").on('click', function (e) {
        PageMethods.SetArch(getCookie("rptID"));
    });
    $('#btnprtP').click(function () {
        $('.dtlD').removeClass('dtlD');
        window.print();
        $('#dtlD>div').addClass('dtlD');
    });
    $.ajax({
        url: "ReportHeader",
        context: document.body,
        success: function (responseText) {
            var hdrData = responseText.split('|');
            $('#txtTitle').html("<h4>" + hdrData[0] + "</h4>");
            $('title').html(hdrData[0]);
            $('#btndash').attr('bit', hdrData[1]);
            if (hdrData[1] == "0") { $('#btndash').addClass('btn-default'); } else { $('#btndash').addClass('btn-success'); }
            $.ajax({
                url: "ReportDTL",
                context: document.body,
                success: function (responseText) {
                    var dtlData = responseText.split('¦');
                    $.each(dtlData, function (index, value) {
                        if (value != "") {
                            var rptSec = value.split('|');
                            $('#dtlD').html($('#dtlD').html() + "<div class='dtlD " + rptSec[13] + "' ><div id=t" + rptSec[10] + " contentEditable class='tRpt'>" + decodeURIComponent(rptSec[3]) + "</div><div style='margin-left: 20px;'>" +
                                "<div class='btn-group float-right'><select tag=" + rptSec[10] + " type='button' class='btn btn-default btnColSize' title='section size settings'><option>" + rptSec[13] + "</option><option>col-md-2</option><option>col-md-3</option><option>col-md-4</option><option>col-md-5</option><option>col-md-6</option><option>col-md-7</option><option>col-md-8</option><option>col-md-9</option><option>col-md-10</option><option>col-md-11</option><option>col-md-12</option></select><button type='button' tag=" + rptSec[10] + " class='btn btn-default DelSec' title='Delete Report Section'><i class='fa-solid fa-trash-can'></i></button></div>" +
                                "</div><div id='c" + rptSec[10] + "' class='chrt' style='margin-top: 20px;'></div>");
                            $.ajax({
                                url: "ReportSection?info='" + encodeURIComponent(rptSec[1] + "','" + rptSec[2] + "','" + rptSec[5] + "':" + rptSec[0] + ":" + rptSec[10]),
                                context: document.body,
                                success: function (responseText) {
                                    $("#c" + rptSec[10]).html(responseText);
                                    var tb = $("#chrt-tb" + rptSec[10]);
                                    if (rptSec[9] == "pie") {
                                        tb.attr('data-graph-datalabels-enabled', '1');
                                        $('#chrt-tb' + rptSec[10] + ' tbody tr').each(function () {
                                            $('td:nth-child(2)', this).attr('data-graph-name', $(':nth-child(1)', this).html());
                                        });
                                    }
                                    if (rptSec[11] == '1') {
                                        tb.attr('data-graph-height', rptSec[12]);
                                        tb.attr('data-graph-type', rptSec[9].replace('bar', 'column')); tb.attr('data-graph-container-before', '1');
                                        if (rptSec[8] == '1') {
                                            tb.attr('data-graph-inverted', '1');
                                            tb.attr('data-graph-xaxis-labels-enabled', '1');
                                        }
                                        if (rptSec[8] == '1') {
                                            tb.attr('data-graph-yaxis-1-stacklabels-enabled', '1');
                                        }
                                        tb.highchartTable();
                                        $('defs').next().hide();
                                        $(window).trigger('resize');
                                    } else { $("#c" + rptSec[10]).css('margin-left', '0px'); }
                                    sumTable("chrt-tb" + rptSec[10]);
                                    if (rptSec[6] == 0) { tb.hide(); } else { $('.tb-css td').each(function () { if ($.isNumeric($(this).html())) { $(this).html(FormatNum($(this).html(), 0)); $(this).css('text-align', 'right'); } }); }
                                }
                            });
                        }
                    });
                }
            });
        }
    });
});