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
        <h2 class="fw-bold m-0">${weatherData.name}<span>${getWeatherIcon(weatherData)}</span></h2>
        <p class="m-0"><span class="fw-bold">Weather:</span> <span class="fst-italic">${weatherData.weather[0].description}</span></p>
        <p class="m-0"><span class="fw-bold">Temperature:</span> <span class="fst-italic">${weatherData.main.temp}°C</span></p>
        <p class="m-0"><span class="fw-bold">Humidity:</span> <span class="fst-italic">${weatherData.main.humidity}%</span></p>
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
