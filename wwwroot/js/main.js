// Variables
if (!localStorage.getItem("mode"))
{
	localStorage.setItem("mode", "dark");
}
var project = 1;
var maxprojects = 5;

// Functions
document.addEventListener("DOMContentLoaded", () =>
{

	// Light/Dark Mode Function
	function LightSwitch()
	{
		// toggles lightmode from all tags in document.body (everything)
		document.body.classList.toggle("lightmode");
	}

	if (localStorage.getItem("mode") === "light") // if the last time we were on the page it was light mode (not default), turn on light mode.
	{
		LightSwitch();
		document.body.classList.add("notransition"); //  without this, you'll see the dark mode briefly as it'll do a transition effect to switch to light mode on the code being ran.
		setTimeout(function () 
		{
			document.body.classList.remove("notransition"); // we don't want it forever because that'll mess up future transitions.
		}, 500);
	}

	document.getElementById("lightswitch").addEventListener("click", () =>
	{
		if (localStorage.getItem("mode") == "dark")
		{
			localStorage.setItem("mode", "light"); // we set it to light mode if it was dark mode
			LightSwitch();
		}
		else if (localStorage.getItem("mode") == "light")
		{
			localStorage.setItem("mode", "dark"); // vice versa of above
			LightSwitch();
		}
	});

	// Time Function

	// This is commented out because we have an identical function in PHP now, that does this better. That being said, we still will have a TheTime function.
	/*function TheTime()
	{
		// time.getMonth()+1 if you use x/x/xxxx for day format
		let time = new Date();
		let months = ["Janurary", "Feburary", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
		let [month, day, year, hour, minute, second, period] = [months[time.getMonth()], time.getDate(), time.getFullYear(), time.getHours(), time.getMinutes(), time.getSeconds(), "AM"];
		// it will only show the minute/second without a 0 in front of it (should it be less than 10) without these two ifs
		if (minute < 10)
		{
			minute = "0" + minute;
		}

		if (second < 10)
		{
			second = "0" + second;
		}

		if (hour >= 12)
		{
			period = "PM";
		}
		else if (hour <= 11)
		{
			period = "AM";
		}

		if (hour > 12)
		{
			// time uses the 24 hour clock, we want a normal clock, so remove 12 hours if it's greater than that.
			hour -= 12;
		}

		if (hour == 0)
		{
			// we don't want midnight showing up as 0.
			hour = 12;
		}

		if (document.getElementById("time")) {
			document.getElementById("time").innerHTML = `Today is ${month} ${day}, ${year} and the time is ${hour}:${minute}:${second} ${period}.`;
		}
	}

	TheTime();*/

	/*function TheTime()
	{
		// Create a fetch request
		fetch("../php/functions.php?function=TheTime")
			.then(response =>
			{
				if(!response.ok)
				{
					console.log(response);
				}
				return response.text();
			})
			.then(data =>
			{
				if(document.getElementById("time"))
				{
					document.getElementById("time").innerHTML = data; // set our time to the response we got from our request.
				}
			})
			.catch(error =>
			{
				console.log(error);
			})
	}
	let timer = setInterval(() => TheTime(), 1000);
	setTimeout(function()
	{
		if(!document.getElementById("time"))
		{
			clearInterval(timer);
		}
	}, 5000);*/

	
	// Slideshow
	// Back Button
	document.getElementById("backbutton").addEventListener("click", () =>
	{
		if (project == 1) // if we are at the first project, go to the last project.
		{
			project = maxprojects;
			document.getElementById("project1").setAttribute("hidden", "");
			document.getElementById("project" + project).removeAttribute("hidden");
		}
		else {
			project--;
			document.getElementById("project" + (project + 1)).setAttribute("hidden", "");
			document.getElementById("project" + project).removeAttribute("hidden");
		}
	});
	// Next Button
	document.getElementById("nextbutton").addEventListener("click", () =>
	{
		if (project == maxprojects) // if we are at the last project, go to the first project.
		{
			project = 1;
			document.getElementById("project" + maxprojects).setAttribute("hidden", "");
			document.getElementById("project" + project).removeAttribute("hidden");
		}
		else {
			project++;
			document.getElementById("project" + (project - 1)).setAttribute("hidden", "");
			document.getElementById("project" + project).removeAttribute("hidden");
		}
	});

	// Links
	document.body.addEventListener("click", (e) =>
	{
		if (e.target.tagName == "A")
		{
			if (e.target.href.includes("http") == false)
			{
				e.preventDefault(); // this is so we don't reload the page on clicking a non http/s link.
			}
		}
	});

	// Typewriter (for the neat header effect)
	let header = document.getElementById("header");
	let i = 0; // iterator
	let text = ""; // text to output
	let speed = 150; // speed of the typewriter in ms
	let thepage = "";
	function TypeWriter()
	{
		if (i < text.length && thepage == localStorage.getItem("lastpage")) // if i < text.length and thepage matches the current page or if thepage is null (first time visiting)
		{
			header.innerHTML += text.charAt(i); // add the letter at i
			i++; // increase iterator
			setTimeout(TypeWriter, speed); // recursively call the function after speed ms
		}
	}
	// uncomment this for every time a user visits the site
	// this is so we don't get <empty string> from trying to get innerHTML too early
	/*setTimeout(() =>
	{
		thepage = localStorage.getItem("lastpage"); // we grab the lastpage so if the page changes we stop the typewriter
		text = header.innerHTML; // set text to header's innerHTML
		header.style.visibility = "visible"; // make header visible
		header.innerHTML = ""; // clear its innerHTML
		TypeWriter(); // call typewriter
	}, 500);*/

// Router (part 1)
	const app = document.getElementById("app");
	
	if(localStorage.getItem("lastpage")) // if we visited the site before, load the last page.
	{
		header.style.visibility = "visible"; // unhide header
		//LoadPage(localStorage.getItem("lastpage"));
	}
	else // first time we were here, load home and play a funny typewriter animation.
	{
		//LoadPage("home");
		// this is so we don't get <empty string> from trying to get innerHTML too early
		setTimeout(() =>
		{
			thepage = localStorage.getItem("lastpage"); // we grab the lastpage so if the page changes we stop the typewriter
			text = header.innerHTML; // set text to header's innerHTML
			header.style.visibility = "visible"; // make header visible
			header.innerHTML = ""; // clear its innerHTML
			TypeWriter(); // call typewriter
		}, 500);
	}
});

// Router (part 2)
/*const pages = "./pages"; // directory of our pages.
const parser = new DOMParser(); // create a DOMParser to parse our pages.
function StringtoHTML(string) // function to convert pages (brought in as strings) to HTML code to put into our app.
{
	return parser.parseFromString(string, "text/html"); 
}
function LoadPage(page) // Load page function, to load the pages into our app
{
	let header = document.getElementById("header"); // header is index.html's header

	fetch(`${pages}/${page}.html`) // fetch the page requested from /pages/
		.then(response => {
			return response.text() // we grab the content of the page, but it is a string
			})
		.then(data => {
			data = StringtoHTML(data); // we make our data HTML from a string, as it should be
			app.innerHTML = data.body.innerHTML; // the div id 'app' will contain our string-to-HTML data from the loaded page.
			let head = document.getElementById("page"); // loaded pages have a h1 header that is the name of the page, with an id of "page"
			header.innerHTML = head.innerHTML; // let's set index.html's header to match the header from the loaded page
			document.title = head.innerHTML; // and let's set our title to the loaded page's name too
			head.parentNode.removeChild(head); // and remove the h1 header from the loaded page, as we don't need duplicates.
			localStorage.setItem("lastpage", page); // and we set the page here in case we reload or come back later.
			/*if (page != "home")
			{
				history.pushState({ page: page }, null, page); // updates url to have /pagename at the end, adds it to history.
			}
			else
			{
				history.pushState(null, null, "/");
			}
		}).catch(error => console.log(error))

// Show/Hide Buttons
	if (page == "portfolio") // if the page is Portfolio, make buttons visible.
	{
		project = 1; // This fixes a bug where it would try to continue from whatever number it was originally on
		// on page switch (if the site wasn't reloaded)
		document.getElementById("buttoncontainer").removeAttribute("hidden");
	}
	else
	{
		document.getElementById("buttoncontainer").setAttribute("hidden", "");
	}
}
*/
// Adds page history
/*window.onpopstate = function (event)
{
	const page = event.state ? event.state.page : "home";
	LoadPage(page);
}*/