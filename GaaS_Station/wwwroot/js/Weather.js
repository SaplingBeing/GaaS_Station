const apiKey = "ef9b791c5a29aeab564a63a0bd9323ef";
const latitude = "-36.8485";
const longitude = "174.7635";

document.addEventListener("DOMContentLoaded", async function () {
    try {
        const weatherData = await getWeatherData();
        displayWeather(weatherData);
    }
    catch (error) {
        console.error(error);
    }
})
async function getWeatherData() {
    const apiUrl = `https://api.openweathermap.org/data/2.5/weather?lat=${latitude}&lon=${longitude}&appid=${apiKey}&units=metric`;
    const response = await fetch(apiUrl);
    console.log(response);
    if (!response.ok) {
        throw new Error("Fetch failed");
    }
    const data = await response.json();
    return data;
}
function displayWeather(weatherData) {
    const weatherContainer = document.querySelector("#weatherContainer");
    weatherContainer.innerHTML = `
        <div class="row">
            <div class="col-4">
                 <p class="text-capitalize fw-bold m-0">${weatherData.weather[0].main}</p>
                 <h1 class="text-center m-0" id="weatherIcon">${getWeatherIcon(weatherData)}</h1>
            </div>
            <div class="col-8 d-flex flex-column justify-content-center">
                <p class="fst-italic m-0"><img src="/images/thermometer-half.svg" alt="Thermometer Icon">${weatherData.main.temp}°C</p>
                <p class="fst-italic m-0"><img src="/images/moisture.svg" alt="Humidity Icon"> ${weatherData.main.humidity}%</p>
                <p class="m-0"><img src="/images/geo-alt.svg" alt="Location Icon"> ${weatherData.name}</p>
            </div>
        </div>
    `;
}
function getWeatherIcon(weatherData) {
    switch (true) {
        case (weatherData.weather[0].id >= 200 && weatherData.weather[0].id < 300):
            return "⛈️"
        case (weatherData.weather[0].id >= 300 && weatherData.weather[0].id < 400):
            return "💦"
        case (weatherData.weather[0].id >= 500 && weatherData.weather[0].id < 600):
            return "🌧️"
        case (weatherData.weather[0].id >= 600 && weatherData.weather[0].id < 700):
            return "❄️"
        case (weatherData.weather[0].id >= 700 && weatherData.weather[0].id < 800):
            return "🍃"
        case (weatherData.weather[0].id === 800):
            return "☀️"
        case (weatherData.weather[0].id >= 801 && weatherData.weather[0].id < 810):
            return "⛅"
        default:
            return "❓"
    }
}
