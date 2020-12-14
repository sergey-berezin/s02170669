async function handleFiles()
{
    try
    {
        var response = await fetch("http://localhost:5000/images/all")
        var json = await response.json()

        if (!json.length)
        {
            fileList.innerHTML = "<p>No files selected!</p>";
        }
        else
        {
            fileList.innerHTML = "";
            const list = document.createElement("ul");
            fileList.appendChild(list);
            for(let i=0; i<json.length; i++)
            {
                const li = document.createElement("li");
                list.appendChild(li);

                var image = new Image();
                image.src = 'data:image/png;base64,' + json[i]["base64Image"];
                image.height = 56;
                image.width = 56;
                li.appendChild(image);
                const info = document.createElement("span");
                info.innerHTML = " is " + json[i]["className"] + " with " + json[i]["prob"] + " prob";
                li.appendChild(info);
            }
        }
    }
    catch(e)
    {
        window.alert(e)
    }
}

async function handleClass()
{
    let TxtClass = document.getElementById("Txt").value;
    try
    {
        var response = await fetch("http://localhost:5000/images/"+TxtClass)
        var json = await response.json()

        if (!json.length)
        {
            fileList.innerHTML = "<p>No files selected!</p>";
        }
        else
        {
            fileList.innerHTML = "";
            const list = document.createElement("ul");
            fileList.appendChild(list);
            for(let i=0; i<json.length; i++)
            {
                const li = document.createElement("li");
                list.appendChild(li);

                var image = new Image();
                image.src = 'data:image/png;base64,' + json[i]["base64Image"];
                image.height = 56;
                image.width = 56;
                li.appendChild(image);
                const info = document.createElement("span");
                info.innerHTML = " is " + json[i]["className"] + " with " + json[i]["prob"] + " prob";
                li.appendChild(info);
            }
        }
    }
    catch(e)
    {
        window.alert(e)
    }
}

async function handleSendFiles()
{
    let SendArray = [];
    const PromiseArray = [];
    const len = this.files.length;
    if (!len)
    {
        fileList.innerHTML = "<p>No files selected!</p>";
    }
    else
    {
        // create list of images
        fileList.innerHTML = "";
        const list = document.createElement("ul");
        fileList.appendChild(list);
        for(let i=0; i<len; i++)
        {   
            PromiseArray.push(new Promise(resolve => 
            {
                // load and display image
            const li = document.createElement("li");
            list.appendChild(li);

            let image = document.createElement("img");
            image.src = URL.createObjectURL(this.files[i]);
            image.id = this.files[i].name;
            image.height = 56;
            image.crossOrigin = 'anonymous';
            image.width = 56;
            image.onload = function()
            {
                    //URL.revokeObjectURL(this.src);
                    resolve();
                
            }
                li.appendChild(image);

                // add image info
                const info = document.createElement("span");
                info.id = "info_"+this.files[i].name;
                info.innerHTML = this.files[i].name;
                li.appendChild(info);

            }));
            
                
        }
        await Promise.all(PromiseArray);

        for(let i=0; i<len; i++)
            {

                let img =  document.getElementById(this.files[i].name);

                // create base64 string from image
                var myCanvas = document.getElementById('mycanvas');
                var ctx = myCanvas.getContext('2d');
                ctx.drawImage(img, 0, 0, img.width*5, img.height*5);
                var myDataURL=myCanvas.toDataURL('image/png');
                var myBase64Data = myDataURL.split(',')[1];
                
                //fill the structure
                let c = {"imageId":0, "imageName":this.files[i].name, "className":"null", "numOfRequests":0,
                        "prob":0, "imageHash":"null", "base64Image":myBase64Data};

                //add image object to the PUT array
                SendArray.push(c);

            }
    }

    try 
    {
        const response = await fetch("http://localhost:5000/images",
        {
            method: 'PUT',
            body: JSON.stringify(SendArray),
            headers: 
            {
            'Content-Type': 'application/json'
            }
        });
        const json = await response.json();

        for(let i=0; i<json.length; i++)
        {
            let inf = document.getElementById("info_"+json[i]["imageName"]);
            inf.innerHTML = " is " + json[i]["className"] + " with " + json[i]["prob"] + " prob";
        }
    }
    catch (error)
    {
    window.alert('Ошибка:', error);
    }

}