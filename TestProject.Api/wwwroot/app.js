// class FileMetadata {
//     name = '';
//     directory = '';
//     extension = '';
//     size = 0;
//     contentType = '';
// } // type class for reference for api call results, not needed since JS is not typed.

class FileService {

    queryFolder(path) {
        let url = '/api/file';
        if (path) {
            url += `?path=${encodeURIComponent(path)}`;
        }
        return fetch(url)
            .then(resp => resp.json());
    }

    downloadUrl(metadata) {
        let url = '/api/file/content';
        if (metadata.directory) {
            url += `?path=${encodeURIComponent(metadata.directory)}/${encodeURIComponent(metadata.name)}`;
        } else {
            url += `?path=${encodeURIComponent(metadata.name)}`;
        }
        return url;
    }

    upload(path, formFile) {
        let url = '/api/file';
        if (path) {
            url += `?path=${encodeURIComponent(path)}`;
        }

        const formData = new FormData();
        formData.append('model', formFile);

        return fetch(url, {
            method: 'POST',
            headers: {},
            body: formData
        }).then(resp => resp.json());
    }
}

class DirectoryBrowserComponent {
    #outlet = undefined;
    #fileSvc = new FileService();
    get Outlet() { return this.#outlet; }

    loadPath() {
        // current state/folder of the directory browser page
        const path = (window.location.hash ?? '').replace('#', '');
        this.#fileSvc.queryFolder(path)
            .then(files => this.renderFiles(files));
    }

    renderFiles(files) {
        let commandBar = this.#outlet.querySelector('.command-bar');
        if (!commandBar) {
            commandBar = document.createElement('div');
            commandBar.innerHTML = `<div class="command-bar">
            <a href="#">
                <span class="material-symbols-outlined">arrow_upward</span>
            </a>
            <span id="currentFolder"></span>
        </div>`;
            this.#outlet.appendChild(commandBar);
        }
        const folderElm = commandBar.querySelector('#currentFolder');
        folderElm.innerText = files.length === 0 ? '' : files[0].directory;

        for (let id = 1; id <= files.length; id++) {
            const file = files[id-1];
            let fileElm = this.#outlet.querySelector(`[app-elm="fil-${id}"]`); // <- prefer a file.Id if we had one from a db
            if (!fileElm) {
                fileElm = document.createElement('div');
                fileElm.setAttribute('app-elm', `fil-${id}`);
                this.#outlet.appendChild(fileElm);
            }
            const isDirectory = file.contentType.toLowerCase() === 'directory';
            const iconName = isDirectory ? 'folder'
                : file.contentType.indexOf('image') >= 0 ? 'image'
                    : 'unknown_document';
            const href = !isDirectory ? this.#fileSvc.downloadUrl(file)
                : !file.directory ? `#${file.name}`
                : `#${file.directory}/${file.name}`;
            const target = isDirectory ? '_self' : '_blank';
            const downloadIconName = isDirectory ? 'arrow_forward' : 'download';
            fileElm.innerHTML = `<div class="details"><span class="material-symbols-outlined">${iconName}</span></span><span>${file.name}</span></div><div class="actions"><a href="${href}" target="${target}"><span class="download material-symbols-outlined">${downloadIconName}</span></a></div>`;
            console.log(fileElm);
        }
        // 3. if an element does not represent a file in the new list, remove the child element
        const appElms = this.#outlet.querySelectorAll(':scope > [app-elm]');
        for (let e = appElms.length - 1; e >= 0; e--) {
            const appElm = appElms[e];
            const id = +appElm.getAttribute('app-elm').replace('fil-', '');
            if (id > files.length) {
                this.#outlet.removeChild(appElm);
            }
        }

    }



    init(outlet) {
        if (!outlet) {
            throw new Error('DirectoryBrowserComponent.Outlet is required');
        }
        this.#outlet = outlet;
        this.loadPath();
        window.addEventListener('hashchange', () => {
            this.loadPath();
        });
    }
}

window.onload = function () {
    // TODO: we could add some routing support with qs parameters to determine the current page and choose a different component
    const page = new DirectoryBrowserComponent(); // we only have one page component
    const outlet = document.getElementById('router-outlet')
    page.init(outlet); // start the component rendering process.
}
