import './App.css';
import { Folders } from './Folders';
import {BrowserRouter, Route, Routes, NavLink, Link} from 'react-router-dom'
import Files from "./Files";

function App() {
  return (
      <div className="App">
        <nav className='navbar navbar-expand-sm bg-light navbar-dark'>
          <ul className='navbar-nav'>
            <li className='nav-item- m-1'>
              <Link className="btn btn-light btn-outline-primary" to="/folders">
                Folders
              </Link>
            </li>
          </ul>
        </nav>

        <Routes>
          <Route path="folders" element={<Folders/>}>
            <Route path=":id" element={<Files/>}/>
          </Route>
        </Routes>
      </div>
  );
}

export default App;
