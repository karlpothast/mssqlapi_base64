const latestSupportedOpenAPIVersion = "https://spec.openapis.org/oas/v3.0.0.html";

(function () {

    console.log("Swagger custom script loaded");

    function initSwaggerCustomizations() {

        console.log('init customizations');

        const info = document.querySelector(".swagger-ui .info");
        if (!info) {
          console.log(' no info element found; return; ');
          return;
        }

        // Reorder title / description / OpenAPI link
        const title = info.querySelector(".title");
        const description = info.querySelector(".description");
        const link = info.querySelector("a[href*='swagger.json']");

        console.log('title : ', title);
        console.log('description : ', description);
        console.log('link : ', link);

        if (title && description && link) {
            title.after(description);
            description.after(link);
            link.innerHTML = `<span class="url">Open API Definition</span>`;
        }

        // Hide top SVG logo
        const svgHeader = document.querySelector(".swagger-ui .topbar svg");
        if (svgHeader) {
            svgHeader.style.display = "none";
        }

        function enhanceOasBadge() {

            const versionPre = document.querySelector(".swagger-ui .version");

            if (!versionPre) return;

            const versionStamp = versionPre.closest(".version-stamp");
            if (!versionStamp) return;

            // Avoid double-wrapping
            if (versionStamp.querySelector("a")) return;

            const link = document.createElement("a");
            link.href = latestSupportedOpenAPIVersion;
            link.target = "_blank";
            link.rel = "noopener noreferrer";

            // Move existing element inside anchor
            versionStamp.parentNode.replaceChild(link, versionStamp);
            link.appendChild(versionStamp);
        }

        enhanceOasBadge();

        console.log("Swagger customizations applied");
    }

    function enableTryMode(opblock) {
        
        setTimeout(() => {
          const tryBtn = opblock.querySelector(".try-out__btn");
        console.log('tryBtn : ', tryBtn);
        if (tryBtn && tryBtn.textContent.includes("Try it out")) {
            tryBtn.click();
        }
        }, 1000);

    }

    function showExecuteButton() {
      execute-wrapperr
      const elements = document.querySelectorAll('.your-class');

      if (elements.length === 1) {
          elements[0].style.display = 'block';
      }
    }

    // Wait until full page load
    window.addEventListener("load", function () {

        const interval = setInterval(() => {

            const swaggerRoot = document.querySelector(".swagger-ui");

            if (swaggerRoot) {
                clearInterval(interval);
                initSwaggerCustomizations();
            }

        }, 100);
    });

    // Auto-enable "Try it out" when an endpoint expands
    document.addEventListener("click", function (e) {

        const summary = e.target.closest(".opblock-summary");
        if (!summary) return;

        requestAnimationFrame(() => {
            const opblock = summary.closest(".opblock");
            if (opblock) {
                enableTryMode(opblock);
            }
        });

    });

})();

//#swagger-ui > section > div.topbar > div > div > a