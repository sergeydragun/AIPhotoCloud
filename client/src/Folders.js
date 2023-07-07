import React, {Component} from "react";
import { consts } from "./Consts";
import {BrowserRouter, Route, Routes, NavLink, Navigate, Link, Outlet } from "react-router-dom";
import { Files } from "./Files";

export class Folders extends Component{
    constructor(props){
        super(props);

        this.state = {
            folders:[],
            Name:"",
            Id:0
        }
    }

    refreshList(){
        fetch(consts.API_URL + "Folders", {
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
            this.setState({folders:data});
        })
        .catch(error => {
            this.setState({folders:[]});
        })
    }

    componentDidMount(){
        this.refreshList();
    }

    createClick(){
        if (this.state.Name == ""){
            return;
        }
        
        console.log(JSON.stringify({
            Name: this.state.Name
        }));

        fetch(consts.API_URL + "Folders", {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"},
            body: JSON.stringify({
                Name: this.state.Name
            })
        })
        .then(response => response.json())
        .then(data=>{
            console.log(data.message);
            this.refreshList();
        },
            (error) => {
                alert(error)
            }
        );

        this.setState({Name:""})
    }
    
    addClick(){
        this.setState({
            Name:""
        })
    }

    changeFolderName = (e) => {
        this.setState({Name:e.target.value})
    }

    openFolderClick(folderName){
        Navigate(`/Folders/${folderName}`)
    }

    MLClick(){
        fetch(consts.API_URL + "ML/CheckPhotos",{
            method: "GET",
            headers: {"Accept": "application/json"},          
        })
        .then(data => {
            this.refreshList();
        });
    }

    render(){
        const {
            folders,
            Name,
            Id
        }=this.state;

        let foldersArray = Array.prototype.slice.call(folders)

        return(
            <div>
                <button type="button"
                className="btn btn-primary m-2 float-end"
                onClick={() => this.MLClick()}>
                    Analyze pictures
                </button>

                <div className='input-group mb-3'>
                    <span className='input-group-text'>Folder's name</span>
                    <input type="text" className='form-control'
                        value={Name}
                        onChange={this.changeFolderName}/>
                    <button type="button"
                    className="btn btn-primary float-start"
                    onClick={()=>this.createClick()}>
                        Create
                    </button>
                </div>           

                <table className='table table-striped'>
                    <thead>
                        <tr>
                            <th>
                                Folders
                            </th>
                        </tr>
                    </thead>
                    <tbody>
                        {foldersArray.map(fold=>
                            <tr key={fold.Id}>
                                <td>
                                    <Link to={`${fold.Id}`}>
                                        {fold.Name}
                                    </Link>
                                </td>
                                <td>
                                    <button type="button">
                                        Edit
                                    </button>
                                </td>
                                <td>
                                    {fold.InFilesCountsInformation != "" ? fold.InFilesCountsInformation : <div>Not recognized yet or objects are missing</div>}
                                </td>
                            </tr>
                            )}
                    </tbody>
                </table>
                <Outlet />
            </div>
        )
    }
}