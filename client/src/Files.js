import React, {Component} from "react";
import { consts } from "./Consts";
import { useParams } from "react-router-dom";
import withRouter from './withRouter';

class Files extends Component{
    constructor(props){
        super(props);

        this.state = {
            files:[],
            Name:"",
            Id:0,
            PhotoPath:consts.PHOTO_URL
        }
    }

    refreshList(){
        fetch(consts.API_URL + "file/" + this.props.params.id, {
            method: "GET",
            headers: {"Accept": "application/json"}
        })
        .then(response => {   
            console.log(response.status);     
            if (response.status == 404){
                throw new Error('error');
            }
            else{
                return response.json()
            }
        })
        .then(data => {
            this.setState({files:data});
        })
        .catch(error => {
            this.setState({files:[]});
        })
    }

    uploadClick(){

    }

    componentDidMount(){
        this.refreshList();
    }

    uploadClick(){
        
    }
    
    changeFolderName = (e) => {
        this.setState({Name:e.target.value})
    }

    imageUpload=(e)=>{
        if(e.target.files[0] == null){
            return
        }

        e.preventDefault();
        const formData = new FormData();
        formData.append("file", e.target.files[0], e.target.files[0].name);
        
        fetch(consts.API_URL + "file/" + this.props.params.id, {
            method:"POST",
            body:formData
        })
        .then(data => {
            this.refreshList()
        })

    }

    reset() {
        this.setState({name: "", price: "", status: "available",desc: "",image: ""})
    }

    imageDownloadClick(file){

        fetch(consts.API_URL + "file/getfile/" + this.props.params.id + "/" + file.Name,{
            method:"GET",
            headers:{
                "Accept": "image/*"
            }
        })
        .then( res => res.blob())
        .then(blob => {
            var image = URL.createObjectURL(blob);

            const anchorElement = document.createElement('a');
            anchorElement.href = image;
            console.log(file.Name)
            anchorElement.download = file.Name;
            document.body.appendChild(anchorElement);
            anchorElement.click();

            document.body.removeChild(anchorElement);
            window.URL.revokeObjectURL(image);
        })
    }

    deleteImage(file){

        fetch(consts.API_URL + "file/" + this.props.params.id + "/" + file.Name,{
            method:"DELETE",
            headers:{
                "Accept": "application/json",
                "Content-Type": "application/json"
            }
        })
        .then( res => {
            this.refreshList();
        })

    }

    render(){
        
        const Id = this.props.params.id
        const {
            files,
            Name,
            PhotoPath
        }=this.state;

        return(
            <div>

                <div className='p-2 w-50 bd-highlight'>
                    <span className='input-group-text'>Files</span>
                    <input className='p-2 w-50 bd-highlight'id="inputUpload" type="file" onChange={this.imageUpload}/>
                </div>
                
                <table className='table table-striped' >
                    <thead>
                        <tr>
                            <th>
                                Files
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        {files.map(file=>
                            <tr key={file.Id}>
                                <td>
                                    <img width="40px" height="40px" 
                                    src={PhotoPath + file.Name}
                                    className="img"/>                                   
                                </td>
                                <td>                                   
                                    <button type="button"
                                    onClick={()=>this.imageDownloadClick(file)}>
                                        {file.Name}
                                    </button>
                                </td> 
                                <td>
                                    <button type="button"
                                    onClick={()=>this.deleteImage(file)}>
                                        Delete
                                    </button>
                                </td>
                                <td>
                                    {file.CountsInformation != "" ? file.CountsInformation : <div>Not recognized yet or objects are missing</div>}
                                </td>
                            </tr>
                            )}
                    </tbody>
                </table>
            </div>
        )
    }
}

export default withRouter(Files); 
 