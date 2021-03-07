// © Microsoft Corporation. All rights reserved.
import React, { useState, useEffect } from 'react';
import GroupCall from './containers/GroupCall';
import { createStore, applyMiddleware } from 'redux';
import { Provider } from 'react-redux';
import { reducer } from './core/reducers';
import thunk from 'redux-thunk';
import TransferRoom from './components/TransferRoom';
import EndCall from './components/EndCall';
import HomeScreen from './components/HomeScreen';
import ConfigurationScreen from './containers/Configuration';
import { v1 as createGUID } from 'uuid';
import { loadTheme, initializeIcons } from '@fluentui/react';
import { utils } from './Utils/Utils';
import { endCall } from 'core/sideEffects';

const sdkVersion = require('../package.json').dependencies['@azure/communication-calling'];
const lastUpdated = `Last Updated ${utils.getBuildTime()} with @azure/communication-calling:${sdkVersion}`;

loadTheme({});
initializeIcons();

const store = createStore(reducer, applyMiddleware(thunk));
const App = () => {
  const [page, setPage] = useState('home');
  const [currentGroupId, setCurrentGroupId] = useState('6d7f87a0-7eca-11eb-ba55-37bd3b8b2fe2');
  const [userGuid, setUserGuid] = useState('');
  const [screenWidth, setScreenWidth] = useState(0);
  const [stateString, setStateString] = useState('still connecting to server');
  window.setInterval(() => {
    try {
      setPollingState(utils.getLobbyStatus(userGuid));
    } catch (e) { }
    }, 3000);
  useEffect(() => {
    const setWindowWidth = () => {
      const width = typeof window !== 'undefined' ? window.innerWidth : 0;
      setScreenWidth(width);
    };
    setWindowWidth();
    window.addEventListener('resize', setWindowWidth);
    return () => window.removeEventListener('resize', setWindowWidth);
  }, []);

  const setPollingState = (lobbyPollObject: any) => {
    lobbyPollObject.then(async function (results: any) {
      let stateString = results["stateString"];
      let currentGroup = currentGroupId;
      let serverRoomId = results["userRoom"];
      setStateString(stateString);
      console.log('page: ' + page);
      if (page === "call" || page === "configuration") {
        setCurrentGroupId(results["userRoom"]);
        if (serverRoomId !== currentGroup) {
          setPage('transfer');
        }
      }
      if (page === "transfer") {
        let currentCall = store.getState().calls.call!;
        if (currentGroupId === serverRoomId) {
          if (currentCall !== undefined) {
            endCall(currentCall, { forEveryone: false });
          }
          setPage('call');
        }
      }
      console.log(results);
    });

  };

  const isUserConfigured = () => {
    const urlParams = new URLSearchParams(window.location.search);
    let configured = urlParams.get('configured');
    if (configured === 'init') {
      return true;
    } else {
      return false;
    }
  };

  const setPageSansTransfer = (pageName:string) => {
    if (page!=="transfer") {
      setPage(pageName);
    }
  };

  const getContent = () => {
    if (page === 'home') {
      return (
        <HomeScreen
          startCallHandler={() => {
            window.history.pushState({}, document.title, window.location.href.split('?')[0] + '?configured=init');
            setPage('call');
          }}
        />
      );
    } else if (page === 'configuration') {
      return (
        <ConfigurationScreen
          startCallHandler={() => setPage('call')}
          unsupportedStateHandler={() => setPage('error')}
          endCallHandler={() => setPageSansTransfer('endCall')}
          groupId={currentGroupId}
          currentGroupId={currentGroupId}
          screenWidth={screenWidth}
        />
      );
    } else if (page === 'call') {
      return (
        <GroupCall
          endCallHandler={() => setPageSansTransfer('endCall')}
          groupId={currentGroupId}
          screenWidth={screenWidth}
          stateString={stateString}
          currentPage={page}
        />
      );
    } else if (page === 'transfer') {
      return (
        <TransferRoom
          message='Transferring you between rooms'
        />
      );
    } else if (page === 'endCall') {
      return (
        <EndCall
          message={ store.getState().calls.attempts > 3 ? 'Unable to join the call' :
          'You left the call'}
          rejoinHandler={() => {
            setPage('call');
          }}
          homeHandler={() => {
            window.location.href = window.location.href.split('?')[0];
          }}
        />
      );
    } else {
      // page === 'error'
      window.document.title = 'Unsupported browser';
      return (
        <>
          <a href="https://docs.microsoft.com/en-us/azure/communication-services/concepts/voice-video-calling/calling-sdk-features#calling-client-library-browser-support">Learn more</a>&nbsp;about
          browsers and platforms supported by the web calling sdk
        </>
      );
    }
  };

  if (isUserConfigured() && page === 'home') {
    setPage('configuration');
  }
  if (userGuid === '') {
    let newGuid = createGUID();
    setUserGuid(newGuid);
    console.log('Set user guid');
  }
  return <Provider store={store}>{getContent()}</Provider>;
};

window.setTimeout(() => {
  try {
    console.log(`Mixer app based on Azure Sample: ${lastUpdated}`);
  } catch (e) {}
}, 0);

export default App;
