// Variables
if (!localStorage.getItem("mode"))
{
	localStorage.setItem("mode", "dark");
}
if (!localStorage.getItem("navbar"))
{
	localStorage.setItem("navbar","closed");
}
// Functions
document.addEventListener("DOMContentLoaded", () =>
{
	// Mobile Check
	if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent))
	{
		let link = document.createElement("link");
		link.href = "./css/mobile.css";
		link.rel = "stylesheet";
		document.head.appendChild(link);
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

	if (localStorage.getItem("navbar") === "open")
	{
		NavbarToggle();
		localStorage.setItem("navbar", "open");
	}
});

// Navbar
function NavbarToggle()
{
	let l = document.getElementById("links");
	l.style.display = l.style.display === "block" ? "none" : "block";
	localStorage.setItem("navbar", localStorage.getItem("navbar") === "closed" ? "open" : "closed");
}