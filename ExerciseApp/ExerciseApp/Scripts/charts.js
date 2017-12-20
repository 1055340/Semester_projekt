$(document).ready(function () {
    $.ajax({
        url: '/Manage/ChartData',
        type: 'Post',
        datatype: 'json',
        data: { order: 'Client_Call' },
        success: function (data) {
            console.log(data.TopExercises);
            console.log(data.TotalKg);
            var JsonArray = data.TopExercises;
            var ExerciseNames = [];
            var ExerciseCount = [];
            
            for (var i = 0; i < JsonArray.length; i++) {
                var obj = JsonArray[i];
                console.log(obj.ExerciseName + ": " + obj.ExerciseCount);
                ExerciseNames[i] = obj.ExerciseName;
                ExerciseCount[i] = obj.ExerciseCount;
                
            };

            var JsonArray2 = data.TotalKg;
            var ExerciseInputs = [];
            var ExerciseInputsnumber = [];
            var increasingTotal = 0;
            var inputNumber = "";
            for (var i = 0; i < JsonArray2.length; i++) {
                var obj2 = JsonArray2[i];
                inputNumber += "";
                ExerciseInputsnumber[i] = inputNumber;
                increasingTotal += obj2.strengthInput;
                console.log(increasingTotal);
                ExerciseInputs[i] = increasingTotal;
            }
            var strengthInputs = data.StrengthInputs;
            var cardioInputs = data.CardioInputs;

        
//global fontcolor = white
Chart.defaults.global.defaultFontColor = 'white';

//Chart.defaults.line.showLines = false;

// global font size = 14
//Chart.defaults.global.defaultFontSize = 12;

Chart.defaults.global.animation.duration = '3000';

//global color
Chart.defaults.global.defaultColor = 'rgba(54, 162, 235, 1)';


//Variables for our charts

var chart1 = document.getElementById("myLineChart");
var myLineChart = new Chart(chart1, {
    type: 'line',
    data: {
        labels: ExerciseInputsnumber,

        //here can u customize your data interface
        datasets: [{
            label: 'Total Lifted KG',

            //linje sharpness
            //lineTension:  0,

            //placeholder data/ default data
            //data: [4, 6, 7, 9, 10],
            data: ExerciseInputs,
            

            backgroundColor: [
                //´transprent backgroung

                'rgba(254,182,69,0.9)'

                //'rgba(255, 255, 255, 0)'
                //'rgba(255, 159, 64, 0.2)'
            ],
            borderColor: [
                //'rgba(255,99,132,1)',
                //'rgba(54, 162, 235, 1)',
                //'rgba(255, 206, 86, 1)',
                //'rgba(75, 192, 192, 1)',
                //'rgba(153, 102, 255, 1)',
                'rgba(255,255,255, 1)'
            ],

            //border linje tykkelse
            borderWidth: 2.5
        }]
    },
    //optional properties to customize

    options: {
        scales: {
            xAxes: [{
                gridLines: {
                    display: false,
                    color: 'rgba(255,255,255, 1)',
                    //borderDash: [4],
                    lineWidth: [3]
                }
            }]
            ,
            yAxes: [{
                gridLines: {
                    display: false,
                    color: 'rgba(255,255,255, 1)',
                    lineWidth: [3]

                },
                ticks: {
                    beginAtZero: true
                }
            }]
        }
    }



});



var chart2 = document.getElementById("myBarChart");
var myBarChart = new Chart(chart2, {
    type: 'bar',
    data: {
        labels: ExerciseNames,

        datasets: [{
            label: 'Top 5 favourite exercises',

            //linje sharpness
            //lineTension:  0,
            //data: [396, 359, 200, 180, 148],
            data: ExerciseCount,
            backgroundColor: [
                //'rgba(255, 99, 132, 0.2)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 159, 64, 0.2)'
                //'rgba(75, 192, 192, 1)'

                //´transprent backgroung
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                'rgba(54, 162, 235, 0)',
                'rgba(54, 162, 235, 0)',
                'rgba(54, 162, 235, 0)',
                'rgba(54, 162, 235, 0)',
                'rgba(54, 162, 235, 0)'



                //'rgba(255, 159, 64, 0.2)'
            ],
            borderColor: [
                //'rgba(255,99,132,1)',
                //'rgba(54, 162, 235, 1)',
                //'rgba(255, 206, 86, 1)',
                //'rgba(75, 192, 192, 1)',
                //'rgba(153, 102, 255, 1)'
                'rgba(255,255,255, 1)',
                'rgba(255,255,255, 1)',
                'rgba(255,255,255, 1)',
                'rgba(255,255,255, 1)',
                'rgba(255,255,255, 1)'



            ],

            //border linje tykkelse
            borderWidth: 2.5
        }]
    },
    options: {

        scales: {
            xAxes: [{
                gridLines: {
                    display: false,
                    color: 'rgba(255,255,255, 1)',
                    //borderDash: [4],
                    lineWidth: [2]
                }
            }]
            ,
            yAxes: [{
                gridLines: {
                    display: false,
                    color: 'rgba(255,255,255, 1)',
                    //borderDash: [4],
                    lineWidth: [2]
                },
                ticks: {
                    beginAtZero: true
                }
            }]
        }
    }



});


var chart3 = document.getElementById("myDonutChart");
var myDonutChart = new Chart(chart3, {
    type: 'pie',
    data: {
        labels: ["Cardio inputs", "Strength inputs"],

        datasets: [{
            label: '# preferences',

            //linje sharpness
            //lineTension:  0,
            //data: [2900, 1308],
            data: [cardioInputs, strengthInputs],
            backgroundColor: [
                //'rgba(255, 99, 132, 0.2)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 159, 64, 0.2)'
                //'rgba(75, 192, 192, 1)'

                //´transprent backgroung
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',
                //'rgba(255, 255, 255, 0)',

                'rgba(54, 123, 235, 1)',
                'rgba(54, 162, 235, 1)'



                //'rgba(255, 159, 64, 0.2)'
            ],
            borderColor: [
                //'rgba(255,99,132,1)',
                //'rgba(54, 162, 235, 1)',
                //'rgba(255, 206, 86, 1)',
                //'rgba(75, 192, 192, 1)',
                //'rgba(153, 102, 255, 1)'

                'rgba(255,255,255, 0)',
                'rgba(255,255,255, 0)'



            ],

            //border linje tykkelse
            borderWidth: 2.5
        }]
    },
    //options: {
    //    scales: {
    //        xAxes: [{
    //            gridLines: {
    //                display: false,
    //                color: 'rgba(255,255,255, 1)',
    //                //borderDash: [4],
    //                lineWidth: [3]
    //            }
    //        }]
    //        ,
    //        yAxes: [{
    //            gridLines: {
    //                display: false,
    //                color: 'rgba(255,255,255, 1)',
    //                //borderDash: [4],
    //                lineWidth: [3]
    //            },
    //            ticks: {
    //                beginAtZero: true
    //            }
    //        }]
    //    }
    //}



});

        },
        error: function (data) {
            console.log("error");
            console.log(data);

        }
    });
});