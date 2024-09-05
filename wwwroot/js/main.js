// Variables
if (!localStorage.getItem("mode"))
{
	localStorage.setItem("mode", "dark");
}
if (!localStorage.getItem("lastpage"))
{
	localStorage.setItem("lastpage","");
}
// Functions
document.addEventListener("DOMContentLoaded", () =>
{
	// Mobile Check
	if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent))
	{
		console.log('mobile');
		let link = document.createElement("link");
		link.href = "./css/mobile.css";
		link.rel = "stylesheet";
		document.head.appendChild(link);
	}
	else
	{
		console.log('not mobile')
	}

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

// Heavily gutted part of Router (part 1) from my normal github website
	const app = document.getElementById("app");
	
	if(localStorage.getItem("lastpage")) // if we visited the site before, load the last page.
	{
		header.style.visibility = "visible"; // unhide header
	}
	else // first time we were here, load home and play a funny typewriter animation.
	{
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